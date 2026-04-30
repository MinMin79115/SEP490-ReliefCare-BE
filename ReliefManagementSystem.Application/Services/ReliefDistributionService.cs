using ReliefManagementSystem.Application.Common.Interface;
using ReliefManagementSystem.Application.Common.Models;
using ReliefManagementSystem.Application.Features.InventoryTransaction.DTOs.Request;
using ReliefManagementSystem.Application.Features.Relief.DTOs.Request;
using ReliefManagementSystem.Application.Features.Relief.DTOs.Response;
using Microsoft.EntityFrameworkCore;
using ReliefManagementSystem.Application.Interface;
using ReliefManagementSystem.Domain.Common;
using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Domain.Enum;
using System.Text.Json;

namespace ReliefManagementSystem.Application.Services
{
    public class ReliefDistributionService : IReliefDistributionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;
        private readonly IInventoryTransactionService _inventoryTransactionService;
        private readonly ICampaignInventoryService _campaignInventoryService;

        public ReliefDistributionService(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUser,
            IInventoryTransactionService inventoryTransactionService,
            ICampaignInventoryService campaignInventoryService)
        {
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
            _inventoryTransactionService = inventoryTransactionService;
            _campaignInventoryService = campaignInventoryService;
        }

        public async Task<IReadOnlyList<CampaignHouseholdResponse>> ImportCampaignHouseholdsAsync(
            Guid campaignId,
            ImportCampaignHouseholdsRequest request,
            CancellationToken cancellationToken = default)
        {
            var campaign = await EnsureReliefCampaignAsync(campaignId, cancellationToken);
            _ = campaign;

            var existing = await _unitOfWork.CampaignHouseholds.GetByCampaignAsync(campaignId, cancellationToken);
            var existingCodes = existing.Select(x => x.HouseholdCode.Trim().ToUpperInvariant()).ToHashSet();

            foreach (var item in request.Households)
            {
                var code = item.HouseholdCode.Trim().ToUpperInvariant();
                if (!existingCodes.Add(code))
                {
                    throw new InvalidOperationException($"Household code '{item.HouseholdCode}' already exists in campaign.");
                }

                var preferredMode = item.DeliveryMode ?? DeliveryMode.PickupAtPoint;
                if (!item.IsIsolated && preferredMode == DeliveryMode.DoorToDoor)
                {
                    throw new InvalidOperationException("Direct delivery is only allowed for isolated households.");
                }

                var household = new CampaignHousehold
                {
                    CampaignHouseholdId = Guid.NewGuid(),
                    CampaignId = campaignId,
                    LocationId = item.LocationId,
                    HouseholdCode = item.HouseholdCode.Trim(),
                    HeadOfHouseholdName = item.HeadOfHouseholdName.Trim(),
                    ContactPhone = item.ContactPhone?.Trim(),
                    Address = item.Address?.Trim(),
                    Latitude = item.Latitude,
                    Longitude = item.Longitude,
                    HouseholdSize = item.HouseholdSize,
                    IsIsolated = item.IsIsolated,
                    FloodSeverityLevel = item.FloodSeverityLevel,
                    IsolationSeverityLevel = item.IsolationSeverityLevel,
                    RequiresBoat = item.RequiresBoat,
                    RequiresLocalGuide = item.RequiresLocalGuide,
                    DeliveryMode = preferredMode,
                    FulfillmentStatus = HouseholdFulfillmentStatus.Pending,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.CampaignHouseholds.AddAsync(household);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            var saved = await _unitOfWork.CampaignHouseholds.GetByCampaignAsync(campaignId, cancellationToken);
            return saved.Select(MapCampaignHousehold).ToList();
        }

        public async Task<CampaignHouseholdResponse> ReportNewReliefHouseholdAsync(
            Guid campaignId,
            ReportNewReliefHouseholdRequest request,
            CancellationToken cancellationToken = default)
        {
            await EnsureReliefCampaignAsync(campaignId, cancellationToken);

            var normalizedCode = request.HouseholdCode.Trim();
            var existing = await _unitOfWork.CampaignHouseholds.GetByCampaignAsync(campaignId, cancellationToken);
            if (existing.Any(x => x.HouseholdCode.Trim().ToUpperInvariant() == normalizedCode.ToUpperInvariant()))
            {
                throw new InvalidOperationException($"Household code '{request.HouseholdCode}' already exists in campaign.");
            }

            var currentUserId = _currentUser.UserId ?? throw new UnauthorizedAccessException("User is not authenticated.");
            var volunteerProfile = await _unitOfWork.VolunteerProfiles.GetByUserIdAsync(currentUserId)
                ?? throw new KeyNotFoundException("Volunteer profile for current user was not found.");

            var teams = await _unitOfWork.Campaigns.GetCampaignTeamsAsync(campaignId, cancellationToken);
            var campaignTeam = teams.FirstOrDefault(x => x.Team.TeamMembers.Any(tm => tm.UserId == currentUserId));

            var household = new CampaignHousehold
            {
                CampaignHouseholdId = Guid.NewGuid(),
                CampaignId = campaignId,
                LocationId = request.LocationId,
                HouseholdCode = normalizedCode,
                HeadOfHouseholdName = request.HeadOfHouseholdName.Trim(),
                ContactPhone = request.ContactPhone?.Trim(),
                Address = request.Address?.Trim(),
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                HouseholdSize = request.HouseholdSize,
                IsIsolated = request.IsIsolated,
                FloodSeverityLevel = request.FloodSeverityLevel,
                IsolationSeverityLevel = request.IsolationSeverityLevel,
                RequiresBoat = request.RequiresBoat,
                RequiresLocalGuide = request.RequiresLocalGuide,
                DeliveryMode = request.IsIsolated ? DeliveryMode.DoorToDoor : DeliveryMode.PickupAtPoint,
                CampaignTeamId = campaignTeam?.CampaignTeamId,
                Notes = request.Notes,
                FulfillmentStatus = HouseholdFulfillmentStatus.Pending,
                CreatedAt = DateTime.UtcNow,
            };

            await _unitOfWork.CampaignHouseholds.AddAsync(household);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return MapCampaignHousehold(household);
        }

        public async Task<HouseholdDeliveryResponse> AssignHouseholdAsync(
            Guid campaignId,
            Guid campaignHouseholdId,
            AssignHouseholdRequest request,
            CancellationToken cancellationToken = default)
        {
            var campaign = await EnsureReliefCampaignAsync(campaignId, cancellationToken);

            var household = await _unitOfWork.CampaignHouseholds.GetByIdAsync(campaignHouseholdId)
                ?? throw new KeyNotFoundException($"Campaign household '{campaignHouseholdId}' was not found.");

            if (household.CampaignId != campaignId)
                throw new InvalidOperationException("Household does not belong to campaign.");

            if (request.DeliveryMode == DeliveryMode.DoorToDoor && !household.IsIsolated)
                throw new InvalidOperationException("Direct delivery is only allowed for isolated households.");

            if (request.DeliveryMode == DeliveryMode.PickupAtPoint && !request.DistributionPointId.HasValue)
                throw new InvalidOperationException("DistributionPointId is required for pickup mode.");

            if (request.DeliveryMode == DeliveryMode.DoorToDoor && !request.CampaignTeamId.HasValue)
                throw new InvalidOperationException("CampaignTeamId is required for door-to-door relief assignment.");

            if (request.DistributionPointId.HasValue)
            {
                var point = await _unitOfWork.DistributionPoints.GetByIdAsync(request.DistributionPointId.Value)
                    ?? throw new KeyNotFoundException($"Distribution point '{request.DistributionPointId}' was not found.");
                if (point.CampaignId != campaignId)
                    throw new InvalidOperationException("Distribution point does not belong to campaign.");
            }

            if (request.CampaignTeamId.HasValue)
            {
                var teams = await _unitOfWork.Campaigns.GetCampaignTeamsAsync(campaignId, cancellationToken);
                if (!teams.Any(t => t.CampaignTeamId == request.CampaignTeamId.Value))
                    throw new KeyNotFoundException($"Campaign team '{request.CampaignTeamId}' was not found in this campaign.");
            }

            var packageId = request.ReliefPackageDefinitionId;
            if (!packageId.HasValue)
            {
                var defaultPackage = await _unitOfWork.ReliefPackageDefinitions.GetDefaultByCampaignAsync(campaignId, cancellationToken)
                    ?? throw new InvalidOperationException("No default relief package found for campaign.");
                packageId = defaultPackage.ReliefPackageDefinitionId;
            }

            var package = await _unitOfWork.ReliefPackageDefinitions.GetByIdAsync(packageId.Value)
                ?? throw new KeyNotFoundException($"Relief package definition '{packageId}' was not found.");
            if (package.CampaignId != campaignId)
                throw new InvalidOperationException("Relief package does not belong to campaign.");
            if (!package.IsActive)
                throw new InvalidOperationException("Relief package is inactive and cannot be assigned.");

            household.DeliveryMode = request.DeliveryMode;
            household.DistributionPointId = request.DeliveryMode == DeliveryMode.PickupAtPoint
                ? request.DistributionPointId
                : null;
            household.CampaignTeamId = request.CampaignTeamId;
            household.Notes = request.Notes;
            household.FulfillmentStatus = HouseholdFulfillmentStatus.Pending;
            await _unitOfWork.CampaignHouseholds.UpdateAsync(household);

            var existingDeliveries = await _unitOfWork.HouseholdDeliveries.GetByCampaignAsync(campaignId, cancellationToken);
            var existingActiveDelivery = existingDeliveries
                .Where(x => x.CampaignHouseholdId == household.CampaignHouseholdId)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefault(x => x.Status != HouseholdFulfillmentStatus.Delivered);

            if (existingActiveDelivery is not null && !request.ForceCreateNewDelivery)
            {
                existingActiveDelivery.DistributionPointId = household.DistributionPointId;
                existingActiveDelivery.CampaignTeamId = request.CampaignTeamId;
                existingActiveDelivery.ReliefPackageDefinitionId = packageId.Value;
                existingActiveDelivery.DeliveryMode = request.DeliveryMode;
                existingActiveDelivery.ScheduledAt = request.ScheduledAt ?? existingActiveDelivery.ScheduledAt;
                existingActiveDelivery.CashSupportAmount = package.CashSupportAmount ?? 0;
                existingActiveDelivery.Notes = request.Notes;
                existingActiveDelivery.Status = HouseholdFulfillmentStatus.Pending;

                await _unitOfWork.HouseholdDeliveries.UpdateAsync(existingActiveDelivery);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                var existingSaved = await _unitOfWork.HouseholdDeliveries.GetByIdWithProofsAsync(existingActiveDelivery.HouseholdDeliveryId, cancellationToken)
                    ?? throw new KeyNotFoundException("Assigned delivery was not found after save.");
                return MapHouseholdDelivery(existingSaved);
            }

            var delivery = new HouseholdDelivery
            {
                HouseholdDeliveryId = Guid.NewGuid(),
                CampaignId = campaignId,
                CampaignHouseholdId = household.CampaignHouseholdId,
                DistributionPointId = household.DistributionPointId,
                CampaignTeamId = request.CampaignTeamId,
                ReliefPackageDefinitionId = packageId.Value,
                DeliveryMode = request.DeliveryMode,
                CashSupportAmount = package.CashSupportAmount ?? 0,
                Status = HouseholdFulfillmentStatus.Pending,
                ScheduledAt = request.ScheduledAt ?? DateTime.UtcNow,
                Notes = request.Notes,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.HouseholdDeliveries.AddAsync(delivery);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var saved = await _unitOfWork.HouseholdDeliveries.GetByIdWithProofsAsync(delivery.HouseholdDeliveryId, cancellationToken)
                ?? throw new KeyNotFoundException("Assigned delivery was not found after save.");
            return MapHouseholdDelivery(saved);
        }

        public async Task<AssignIsolatedHouseholdTeamResponse> AssignIsolatedHouseholdTeamAsync(
            Guid campaignId,
            Guid campaignHouseholdId,
            AssignIsolatedHouseholdTeamRequest request,
            CancellationToken cancellationToken = default)
        {
            await EnsureReliefCampaignAsync(campaignId, cancellationToken);

            var household = await _unitOfWork.CampaignHouseholds.GetQueryable()
                .Include(x => x.CampaignTeam)
                    .ThenInclude(ct => ct.Team)
                .Include(x => x.Location)
                .FirstOrDefaultAsync(x => x.CampaignHouseholdId == campaignHouseholdId, cancellationToken)
                ?? throw new KeyNotFoundException($"Campaign household '{campaignHouseholdId}' was not found.");

            if (household.CampaignId != campaignId)
                throw new InvalidOperationException("Household does not belong to campaign.");

            if (!household.IsIsolated)
                throw new InvalidOperationException("Only isolated households can be assigned with this endpoint.");

            var teams = await _unitOfWork.Campaigns.GetCampaignTeamsAsync(campaignId, cancellationToken);
            if (!teams.Any(t => t.CampaignTeamId == request.CampaignTeamId))
                throw new KeyNotFoundException($"Campaign team '{request.CampaignTeamId}' was not found in this campaign.");

            var packageId = request.ReliefPackageDefinitionId;
            if (!packageId.HasValue)
            {
                var defaultPackage = await _unitOfWork.ReliefPackageDefinitions.GetDefaultByCampaignAsync(campaignId, cancellationToken)
                    ?? throw new InvalidOperationException("No default relief package found for campaign.");
                packageId = defaultPackage.ReliefPackageDefinitionId;
            }

            var package = await _unitOfWork.ReliefPackageDefinitions.GetByIdAsync(packageId.Value)
                ?? throw new KeyNotFoundException($"Relief package definition '{packageId}' was not found.");

            if (package.CampaignId != campaignId)
                throw new InvalidOperationException("Relief package does not belong to campaign.");

            if (!package.IsActive)
                throw new InvalidOperationException("Relief package is inactive and cannot be assigned.");

            household.CampaignTeamId = request.CampaignTeamId;
            household.DeliveryMode = request.KeepDoorToDoor ? DeliveryMode.DoorToDoor : household.DeliveryMode;
            household.DistributionPointId = null;
            household.FulfillmentStatus = HouseholdFulfillmentStatus.Pending;
            household.Notes = string.IsNullOrWhiteSpace(request.Notes)
                ? household.Notes
                : string.Join(" | ", new[] { household.Notes, request.Notes?.Trim() }.Where(x => !string.IsNullOrWhiteSpace(x)));

            await _unitOfWork.CampaignHouseholds.UpdateAsync(household);

            var existingDeliveries = await _unitOfWork.HouseholdDeliveries.GetByCampaignAsync(campaignId, cancellationToken);
            var existingActiveDelivery = existingDeliveries
                .Where(x => x.CampaignHouseholdId == household.CampaignHouseholdId)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefault(x => x.Status != HouseholdFulfillmentStatus.Delivered);

            HouseholdDelivery activeDelivery;

            if (existingActiveDelivery is not null && !request.ForceCreateNewDelivery)
            {
                existingActiveDelivery.CampaignTeamId = request.CampaignTeamId;
                existingActiveDelivery.DistributionPointId = null;
                existingActiveDelivery.DeliveryMode = DeliveryMode.DoorToDoor;
                existingActiveDelivery.ScheduledAt = request.ScheduledAt ?? existingActiveDelivery.ScheduledAt;
                existingActiveDelivery.CashSupportAmount = package.CashSupportAmount ?? 0;
                existingActiveDelivery.Status = HouseholdFulfillmentStatus.Pending;
                if (!string.IsNullOrWhiteSpace(request.Notes))
                    existingActiveDelivery.Notes = request.Notes?.Trim();

                existingActiveDelivery.ReliefPackageDefinitionId = packageId.Value;

                await _unitOfWork.HouseholdDeliveries.UpdateAsync(existingActiveDelivery);
                activeDelivery = existingActiveDelivery;
            }
            else
            {
                activeDelivery = new HouseholdDelivery
                {
                    HouseholdDeliveryId = Guid.NewGuid(),
                    CampaignId = campaignId,
                    CampaignHouseholdId = household.CampaignHouseholdId,
                    DistributionPointId = null,
                    CampaignTeamId = request.CampaignTeamId,
                    ReliefPackageDefinitionId = packageId.Value,
                    DeliveryMode = DeliveryMode.DoorToDoor,
                    CashSupportAmount = package.CashSupportAmount ?? 0,
                    Status = HouseholdFulfillmentStatus.Pending,
                    ScheduledAt = request.ScheduledAt ?? DateTime.UtcNow,
                    Notes = request.Notes,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.HouseholdDeliveries.AddAsync(activeDelivery);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            var savedDelivery = await _unitOfWork.HouseholdDeliveries.GetByIdWithProofsAsync(activeDelivery.HouseholdDeliveryId, cancellationToken)
                ?? throw new KeyNotFoundException("Assigned isolated delivery was not found after save.");

            return new AssignIsolatedHouseholdTeamResponse
            {
                Household = MapCampaignHousehold(household),
                Delivery = MapHouseholdDelivery(savedDelivery)
            };
        }

        public async Task<BulkAssignIsolatedHouseholdsResponse> BulkAssignIsolatedHouseholdTeamsAsync(
            Guid campaignId,
            BulkAssignIsolatedHouseholdsRequest request,
            CancellationToken cancellationToken = default)
        {
            await EnsureReliefCampaignAsync(campaignId, cancellationToken);

            var response = new BulkAssignIsolatedHouseholdsResponse
            {
                TotalRequested = request.CampaignHouseholdIds.Count
            };

            foreach (var householdId in request.CampaignHouseholdIds.Distinct())
            {
                try
                {
                    var result = await AssignIsolatedHouseholdTeamAsync(campaignId, householdId, new AssignIsolatedHouseholdTeamRequest
                    {
                        CampaignTeamId = request.CampaignTeamId,
                        ReliefPackageDefinitionId = request.ReliefPackageDefinitionId,
                        ScheduledAt = request.ScheduledAt,
                        KeepDoorToDoor = request.KeepDoorToDoor,
                        Notes = request.Notes,
                        ForceCreateNewDelivery = request.ForceCreateNewDelivery
                    }, cancellationToken);

                    response.Items.Add(new BulkAssignIsolatedHouseholdItemResponse
                    {
                        CampaignHouseholdId = householdId,
                        IsSuccess = true,
                        Household = result.Household,
                        Delivery = result.Delivery
                    });
                }
                catch (Exception ex)
                {
                    response.Items.Add(new BulkAssignIsolatedHouseholdItemResponse
                    {
                        CampaignHouseholdId = householdId,
                        IsSuccess = false,
                        Error = ex.Message
                    });
                }
            }

            response.SuccessCount = response.Items.Count(x => x.IsSuccess);
            response.FailureCount = response.Items.Count(x => !x.IsSuccess);
            return response;
        }

        public async Task<Pagination<CampaignHouseholdResponse>> GetCampaignHouseholdsAsync(
            Guid campaignId,
            HouseholdQueryRequest request,
            CancellationToken cancellationToken = default)
        {
            await EnsureReliefCampaignAsync(campaignId, cancellationToken);

            IQueryable<CampaignHousehold> query = _unitOfWork.CampaignHouseholds.GetQueryable()
                .Include(x => x.DistributionPoint)
                .Include(x => x.CampaignTeam)
                    .ThenInclude(ct => ct.Team)
                .Where(x => x.CampaignId == campaignId);

            if (request.Status.HasValue)
                query = query.Where(x => x.FulfillmentStatus == request.Status.Value);

            if (request.DeliveryMode.HasValue)
                query = query.Where(x => x.DeliveryMode == request.DeliveryMode.Value);

            if (request.DistributionPointId.HasValue)
                query = query.Where(x => x.DistributionPointId == request.DistributionPointId.Value);

            if (request.CampaignTeamId.HasValue)
                query = query.Where(x => x.CampaignTeamId == request.CampaignTeamId.Value);

            if (request.IsAssigned.HasValue)
                query = request.IsAssigned.Value
                    ? query.Where(x => x.CampaignTeamId != null)
                    : query.Where(x => x.CampaignTeamId == null);

            if (request.IsIsolated.HasValue)
                query = query.Where(x => x.IsIsolated == request.IsIsolated.Value);

            if (request.RequiresBoat.HasValue)
                query = query.Where(x => x.RequiresBoat == request.RequiresBoat.Value);

            if (request.RequiresLocalGuide.HasValue)
                query = query.Where(x => x.RequiresLocalGuide == request.RequiresLocalGuide.Value);

            if (request.MinFloodSeverityLevel.HasValue)
                query = query.Where(x => (x.FloodSeverityLevel ?? 0) >= request.MinFloodSeverityLevel.Value);

            if (request.MinIsolationSeverityLevel.HasValue)
                query = query.Where(x => (x.IsolationSeverityLevel ?? 0) >= request.MinIsolationSeverityLevel.Value);

            if (request.HasCoordinates.HasValue)
            {
                query = request.HasCoordinates.Value
                    ? query.Where(x => x.Latitude != 0 || x.Longitude != 0)
                    : query.Where(x => x.Latitude == 0 && x.Longitude == 0);
            }

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var keyword = request.Search.Trim();
                query = query.Where(x =>
                    x.HouseholdCode.Contains(keyword) ||
                    x.HeadOfHouseholdName.Contains(keyword) ||
                    (x.ContactPhone ?? string.Empty).Contains(keyword) ||
                    (x.Address ?? string.Empty).Contains(keyword));
            }

            var pageIndex = request.PageIndex <= 0 ? 1 : request.PageIndex;
            var pageSize = request.PageSize <= 0 ? 10 : request.PageSize;
            query = query.OrderByDescending(x => x.CreatedAt);
            var paged = await Pagination<CampaignHousehold>.ToPagedList(query, pageIndex, pageSize);
            var items = paged.Items!.Select(MapCampaignHousehold).ToList();

            return new Pagination<CampaignHouseholdResponse>(items, paged.TotalCount, paged.CurrentPage, paged.PageSize);
        }

        public async Task<ReliefCampaignPlanSummaryResponse> GetCampaignPlanSummaryAsync(
            Guid campaignId,
            CancellationToken cancellationToken = default)
        {
            await EnsureReliefCampaignAsync(campaignId, cancellationToken);

            var households = await _unitOfWork.CampaignHouseholds.GetQueryable()
                .Include(x => x.CampaignTeam)
                    .ThenInclude(ct => ct.Team)
                .Include(x => x.Location)
                .Where(x => x.CampaignId == campaignId)
                .ToListAsync(cancellationToken);

            var activeLocations = await _unitOfWork.Locations.GetAllActiveAsync();

            var distributionPoints = await _unitOfWork.DistributionPoints.GetQueryable()
                .Include(x => x.Households)
                .Include(x => x.Deliveries)
                .Where(x => x.CampaignId == campaignId)
                .ToListAsync(cancellationToken);

            var totalHouseholds = households.Count;
            var isolatedHouseholds = households.Count(x => x.IsIsolated);
            var totalPopulation = households.Sum(x => x.HouseholdSize);
            var pendingHouseholds = households.Count(x => x.FulfillmentStatus != HouseholdFulfillmentStatus.Delivered);
            var floodSeverityScore = households
                .Where(x => x.FloodSeverityLevel.HasValue)
                .Select(x => x.FloodSeverityLevel!.Value)
                .DefaultIfEmpty(0)
                .Average();
            var areaLocationDensities = households
                .Where(x => x.Location != null)
                .Select(x => x.Location!.PopulationDensity)
                .Where(x => x > 0)
                .ToList();

            var averagePopulationDensity = areaLocationDensities.Count > 0
                ? Math.Round(areaLocationDensities.Average(), 2)
                : 0;

            if (averagePopulationDensity <= 0)
            {
                var inferredDensities = households
                    .Select(h => ResolveLocationContext([h], activeLocations)?.PopulationDensity ?? 0)
                    .Where(x => x > 0)
                    .ToList();
                if (inferredDensities.Count > 0)
                    averagePopulationDensity = Math.Round(inferredDensities.Average(), 2);
            }

            var suggestedTeamCount = CalculateSuggestedTeamCount(
                totalHouseholds,
                isolatedHouseholds,
                totalPopulation,
                averagePopulationDensity,
                households);
            var estimatedReliefPersonnel = CalculateEstimatedReliefPersonnel(suggestedTeamCount, totalPopulation);
            var estimatedLocalVolunteers = CalculateEstimatedLocalVolunteers(isolatedHouseholds);
            var estimatedBoatCount = CalculateEstimatedBoatCount(isolatedHouseholds, floodSeverityScore, households.Count(x => x.RequiresBoat));
            var estimatedLifeJacketCount = CalculateEstimatedLifeJacketCount(
                estimatedReliefPersonnel,
                estimatedLocalVolunteers,
                estimatedBoatCount);

            var areaGroups = BuildAreaGroups(households);

            var areaSummaries = areaGroups
                .Select(group =>
                {
                    var groupItems = group.ToList();
                    var resolvedLocation = ResolveLocationContext(groupItems, activeLocations);
                    var householdCount = groupItems.Count;
                    var areaIsolatedCount = groupItems.Count(x => x.IsIsolated);
                    var areaPopulation = groupItems.Sum(x => x.HouseholdSize);
                    var areaPendingCount = groupItems.Count(x => x.FulfillmentStatus != HouseholdFulfillmentStatus.Delivered);
                    var areaDensity = group
                        .Select(x => x.Location?.PopulationDensity ?? 0)
                        .FirstOrDefault(x => x > 0);
                    if (areaDensity <= 0)
                        areaDensity = resolvedLocation?.PopulationDensity ?? 0;
                    var areaFloodSeverity = groupItems
                        .Where(x => x.FloodSeverityLevel.HasValue)
                        .Select(x => x.FloodSeverityLevel!.Value)
                        .DefaultIfEmpty(0)
                        .Average();
                    var areaRadiusKm = EstimateCoverageRadiusKm(groupItems);
                    var areaSuggestedTeams = CalculateSuggestedTeamCount(
                        householdCount,
                        areaIsolatedCount,
                        areaPopulation,
                        areaDensity,
                        groupItems);
                    var areaEstimatedBoats = CalculateEstimatedBoatCount(areaIsolatedCount, areaFloodSeverity, groupItems.Count(x => x.RequiresBoat));
                    var areaEstimatedLocalVolunteers = CalculateEstimatedLocalVolunteers(areaIsolatedCount);
                    var areaAverageHouseholdSize = householdCount > 0
                        ? Math.Round((decimal)areaPopulation / householdCount, 2)
                        : 0;
                    var recommendedOperationalMode = GetRecommendedOperationalMode(areaDensity, areaRadiusKm, areaIsolatedCount);
                    var suggestedDistributionPointCount = CalculateSuggestedDistributionPointCount(householdCount, areaDensity, areaRadiusKm, areaIsolatedCount);
                    var suggestedMobileTeamCount = CalculateSuggestedMobileTeamCount(areaSuggestedTeams, areaDensity, areaRadiusKm, areaIsolatedCount);

                    return new ReliefPlanAreaSummaryResponse
                    {
                        AreaName = ResolveAreaName(groupItems),
                        LocationId = groupItems.FirstOrDefault(x => x.LocationId.HasValue)?.LocationId ?? resolvedLocation?.LocationId,
                        MatchedLocationName = resolvedLocation?.FullName,
                        LocationMatchSource = resolvedLocation is null ? null : "address-latlng-heuristic",
                        PopulationDensity = areaDensity,
                        HouseholdCount = householdCount,
                        IsolatedHouseholdCount = areaIsolatedCount,
                        Population = areaPopulation,
                        AverageHouseholdSize = areaAverageHouseholdSize,
                        PendingHouseholds = areaPendingCount,
                        EstimatedCoverageRadiusKm = Math.Round(areaRadiusKm, 2),
                        TravelComplexityLabel = GetTravelComplexityLabel(areaRadiusKm, areaDensity, areaIsolatedCount),
                        RecommendedOperationalMode = recommendedOperationalMode,
                        RecommendedDeliveryStrategy = recommendedOperationalMode == "Ưu tiên điểm phát"
                            ? "Tập trung phát tại điểm, gom hộ gần nhau"
                            : "Đội cơ động gõ từng cụm, giao tận nơi cho hộ cô lập",
                        SuggestedPeoplePerTeam = CalculateSuggestedPeoplePerTeam(areaSuggestedTeams, areaPopulation),
                        SuggestedPeoplePerDistributionPointLine = CalculateSuggestedPeoplePerLine(householdCount, areaDensity),
                        SuggestedDistributionPointCount = suggestedDistributionPointCount,
                        SuggestedMobileTeamCount = suggestedMobileTeamCount,
                        SuggestedTeamCount = areaSuggestedTeams,
                        EstimatedPackages = areaPendingCount,
                        EstimatedBoatCount = areaEstimatedBoats,
                        EstimatedLifeJacketCount = CalculateEstimatedLifeJacketCount(
                            CalculateEstimatedReliefPersonnel(areaSuggestedTeams, areaPopulation),
                            areaEstimatedLocalVolunteers,
                            areaEstimatedBoats),
                    };
                })
                .OrderByDescending(x => x.IsolatedHouseholdCount)
                .ThenByDescending(x => x.HouseholdCount)
                .ThenBy(x => x.AreaName)
                .ToList();

            var isolatedItems = households
                .Where(x => x.IsIsolated)
                .OrderByDescending(x => x.HouseholdSize)
                .ThenBy(x => x.HeadOfHouseholdName)
                .Select(x => new IsolatedHouseholdPlanItemResponse
                {
                    CampaignHouseholdId = x.CampaignHouseholdId,
                    CampaignTeamId = x.CampaignTeamId,
                    HouseholdCode = x.HouseholdCode,
                    HeadOfHouseholdName = x.HeadOfHouseholdName,
                    Address = x.Address,
                    LocationId = x.LocationId,
                    Latitude = x.Latitude,
                    Longitude = x.Longitude,
                    HouseholdSize = x.HouseholdSize,
                    FloodSeverityLevel = x.FloodSeverityLevel,
                    IsolationSeverityLevel = x.IsolationSeverityLevel,
                    RequiresBoat = x.RequiresBoat,
                    RequiresLocalGuide = x.RequiresLocalGuide,
                    PriorityLabel = GetPriorityLabel(x.HouseholdSize),
                    SuggestedSupportMode = GetSuggestedSupportMode(x),
                    EstimatedReliefPersonnel = Math.Max(2, (int)Math.Ceiling(x.HouseholdSize / 2.0)),
                    EstimatedBoatCount = CalculateEstimatedBoatCount(1, x.FloodSeverityLevel ?? 0, x.RequiresBoat ? 1 : 0),
                    EstimatedLifeJacketCount = Math.Max(2, x.HouseholdSize),
                    CampaignTeamName = x.CampaignTeam?.Team?.Name
                })
                .ToList();

            var distributionPointSummaries = distributionPoints
                .Select(x => new DistributionPointPlanSummaryResponse
                {
                    DistributionPointId = x.DistributionPointId,
                    Name = x.Name,
                    Address = x.Address,
                    AssignedHouseholdCount = x.Households.Count,
                    PendingDeliveryCount = x.Deliveries.Count(d => d.Status != HouseholdFulfillmentStatus.Delivered),
                    SuggestedPersonnelCount = Math.Max(2, (int)Math.Ceiling(x.Households.Count / 25.0)),
                    SuggestedLocalVolunteerCount = Math.Max(0, (int)Math.Ceiling(x.Households.Count(h => h.IsIsolated) / 5.0))
                })
                .OrderByDescending(x => x.PendingDeliveryCount)
                .ThenBy(x => x.Name)
                .ToList();

            var resourceRequirements = new List<ReliefResourceRequirementResponse>
            {
                new()
                {
                    ResourceType = "Nhân lực",
                    ResourceName = "Đội cứu trợ",
                    EstimatedQuantity = suggestedTeamCount,
                    Notes = "Gợi ý 1 đội cho mỗi 50 hộ cần hỗ trợ."
                },
                new()
                {
                    ResourceType = "Nhân lực",
                    ResourceName = "Nhân sự cứu trợ",
                    EstimatedQuantity = estimatedReliefPersonnel,
                    Notes = "Ước tính theo quy mô dân số và số đội."
                },
                new()
                {
                    ResourceType = "Nhân lực",
                    ResourceName = "TNV địa phương / dẫn đường",
                    EstimatedQuantity = estimatedLocalVolunteers,
                    Notes = "Ưu tiên bổ sung cho khu vực có hộ bị cô lập."
                },
                new()
                {
                    ResourceType = "Thiết bị",
                    ResourceName = "Xuồng / ghe tiếp cận",
                    EstimatedQuantity = estimatedBoatCount,
                    Notes = "Gợi ý theo số hộ cô lập."
                },
                new()
                {
                    ResourceType = "Thiết bị",
                    ResourceName = "Áo phao",
                    EstimatedQuantity = estimatedLifeJacketCount,
                    Notes = "Ước tính cho nhân lực chính, TNV địa phương và tổ tiếp cận."
                },
                new()
                {
                    ResourceType = "Hàng cứu trợ",
                    ResourceName = "Gói cứu trợ dự kiến",
                    EstimatedQuantity = pendingHouseholds,
                    Notes = "Tạm tính mỗi hộ chờ phát tương ứng 1 gói cứu trợ."
                }
            };

            return new ReliefCampaignPlanSummaryResponse
            {
                CampaignId = campaignId,
                TotalHouseholds = totalHouseholds,
                IsolatedHouseholds = isolatedHouseholds,
                TotalPopulation = totalPopulation,
                AveragePopulationDensity = averagePopulationDensity,
                SuggestedPeoplePerTeam = CalculateSuggestedPeoplePerTeam(suggestedTeamCount, totalPopulation),
                SuggestedPeoplePerDistributionPointLine = CalculateSuggestedPeoplePerLine(totalHouseholds, averagePopulationDensity),
                HighDensityAreaCount = areaSummaries.Count(x => x.PopulationDensity >= 1000),
                MobileTeamPriorityAreaCount = areaSummaries.Count(x => x.RecommendedOperationalMode == "Ưu tiên đội cơ động"),
                PickupPriorityAreaCount = areaSummaries.Count(x => x.RecommendedOperationalMode == "Ưu tiên điểm phát"),
                DistributionPointCount = distributionPoints.Count,
                PendingHouseholds = pendingHouseholds,
                SuggestedTeamCount = suggestedTeamCount,
                EstimatedReliefPersonnel = estimatedReliefPersonnel,
                EstimatedLocalVolunteers = estimatedLocalVolunteers,
                EstimatedBoatCount = estimatedBoatCount,
                EstimatedLifeJacketCount = estimatedLifeJacketCount,
                Areas = areaSummaries,
                IsolatedHouseholdItems = isolatedItems,
                DistributionPoints = distributionPointSummaries,
                ResourceRequirements = resourceRequirements,
            };
        }

        public async Task<CampaignHouseholdResponse> UpdateCampaignHouseholdAsync(
            Guid campaignId,
            Guid campaignHouseholdId,
            UpdateCampaignHouseholdRequest request,
            CancellationToken cancellationToken = default)
        {
            await EnsureReliefCampaignAsync(campaignId, cancellationToken);

            var household = await _unitOfWork.CampaignHouseholds.GetByIdWithDeliveriesAsync(campaignHouseholdId, cancellationToken)
                ?? throw new KeyNotFoundException($"Campaign household '{campaignHouseholdId}' was not found.");

            if (household.CampaignId != campaignId)
                throw new InvalidOperationException("Household does not belong to campaign.");

            if (!string.IsNullOrWhiteSpace(request.HouseholdCode))
            {
                var normalized = request.HouseholdCode.Trim().ToUpperInvariant();
                var duplicated = await _unitOfWork.CampaignHouseholds.GetByCampaignAsync(campaignId, cancellationToken);
                if (duplicated.Any(x => x.CampaignHouseholdId != campaignHouseholdId && x.HouseholdCode.Trim().ToUpperInvariant() == normalized))
                    throw new InvalidOperationException($"Household code '{request.HouseholdCode}' already exists in campaign.");

                household.HouseholdCode = request.HouseholdCode.Trim();
            }

            if (!string.IsNullOrWhiteSpace(request.HeadOfHouseholdName))
                household.HeadOfHouseholdName = request.HeadOfHouseholdName.Trim();

            if (request.ContactPhone is not null)
                household.ContactPhone = string.IsNullOrWhiteSpace(request.ContactPhone) ? null : request.ContactPhone.Trim();

            if (request.Address is not null)
                household.Address = string.IsNullOrWhiteSpace(request.Address) ? null : request.Address.Trim();

            if (request.Latitude.HasValue)
                household.Latitude = request.Latitude.Value;

            if (request.Longitude.HasValue)
                household.Longitude = request.Longitude.Value;

            if (request.LocationId.HasValue)
                household.LocationId = request.LocationId.Value;

            if (request.HouseholdSize.HasValue)
            {
                if (request.HouseholdSize.Value <= 0)
                    throw new InvalidOperationException("Household size must be greater than 0.");
                household.HouseholdSize = request.HouseholdSize.Value;
            }

            if (request.IsIsolated.HasValue)
                household.IsIsolated = request.IsIsolated.Value;

            if (request.FloodSeverityLevel.HasValue)
                household.FloodSeverityLevel = request.FloodSeverityLevel.Value;

            if (request.IsolationSeverityLevel.HasValue)
                household.IsolationSeverityLevel = request.IsolationSeverityLevel.Value;

            if (request.RequiresBoat.HasValue)
                household.RequiresBoat = request.RequiresBoat.Value;

            if (request.RequiresLocalGuide.HasValue)
                household.RequiresLocalGuide = request.RequiresLocalGuide.Value;

            var nextDeliveryMode = request.DeliveryMode ?? household.DeliveryMode;
            if (!household.IsIsolated && nextDeliveryMode == DeliveryMode.DoorToDoor)
                throw new InvalidOperationException("Direct delivery is only allowed for isolated households.");

            if (request.DistributionPointId.HasValue)
            {
                var point = await _unitOfWork.DistributionPoints.GetByIdAsync(request.DistributionPointId.Value)
                    ?? throw new KeyNotFoundException($"Distribution point '{request.DistributionPointId.Value}' was not found.");
                if (point.CampaignId != campaignId)
                    throw new InvalidOperationException("Distribution point does not belong to campaign.");
                household.DistributionPointId = request.DistributionPointId;
            }

            if (request.CampaignTeamId.HasValue)
            {
                var teams = await _unitOfWork.Campaigns.GetCampaignTeamsAsync(campaignId, cancellationToken);
                if (!teams.Any(t => t.CampaignTeamId == request.CampaignTeamId.Value))
                    throw new KeyNotFoundException($"Campaign team '{request.CampaignTeamId.Value}' was not found in this campaign.");
                household.CampaignTeamId = request.CampaignTeamId;
            }

            household.DeliveryMode = nextDeliveryMode;
            if (nextDeliveryMode == DeliveryMode.DoorToDoor)
                household.DistributionPointId = null;

            if (request.Notes is not null)
                household.Notes = request.Notes;

            await _unitOfWork.CampaignHouseholds.UpdateAsync(household);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return MapCampaignHousehold(household);
        }

        public async Task<CampaignHouseholdResponse> UpdateCampaignHouseholdStatusAsync(
            Guid campaignId,
            Guid campaignHouseholdId,
            UpdateCampaignHouseholdStatusRequest request,
            CancellationToken cancellationToken = default)
        {
            await EnsureReliefCampaignAsync(campaignId, cancellationToken);

            var household = await _unitOfWork.CampaignHouseholds.GetByIdWithDeliveriesAsync(campaignHouseholdId, cancellationToken)
                ?? throw new KeyNotFoundException($"Campaign household '{campaignHouseholdId}' was not found.");

            if (household.CampaignId != campaignId)
                throw new InvalidOperationException("Household does not belong to campaign.");

            household.FulfillmentStatus = request.Status;

            if (request.Notes is not null)
                household.Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim();

            await _unitOfWork.CampaignHouseholds.UpdateAsync(household);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return MapCampaignHousehold(household);
        }

        public async Task DeleteCampaignHouseholdAsync(
            Guid campaignId,
            Guid campaignHouseholdId,
            CancellationToken cancellationToken = default)
        {
            await EnsureReliefCampaignAsync(campaignId, cancellationToken);

            var household = await _unitOfWork.CampaignHouseholds.GetByIdWithDeliveriesAsync(campaignHouseholdId, cancellationToken)
                ?? throw new KeyNotFoundException($"Campaign household '{campaignHouseholdId}' was not found.");

            if (household.CampaignId != campaignId)
                throw new InvalidOperationException("Household does not belong to campaign.");

            if (household.Deliveries.Any())
                throw new InvalidOperationException("Cannot delete household that already has delivery records.");

            await _unitOfWork.CampaignHouseholds.DeleteAsync(household);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task<Pagination<HouseholdChecklistItemResponse>> GetChecklistAsync(
            Guid campaignId,
            DeliveryQueryRequest request,
            CancellationToken cancellationToken = default)
        {
            await EnsureReliefCampaignAsync(campaignId, cancellationToken);
            IQueryable<HouseholdDelivery> query = (await _unitOfWork.HouseholdDeliveries.GetByChecklistAsync(
                campaignId,
                request.CampaignTeamId,
                request.Status,
                cancellationToken)).AsQueryable();

            query = ApplyDeliveryFilters(query, request);

            var pageIndex = request.PageIndex <= 0 ? 1 : request.PageIndex;
            var pageSize = request.PageSize <= 0 ? 10 : request.PageSize;
            var ordered = query.OrderBy(x => x.ScheduledAt).ToList();
            var totalCount = ordered.Count;
            var items = ordered
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new HouseholdChecklistItemResponse
                {
                    HouseholdDeliveryId = x.HouseholdDeliveryId,
                    CampaignId = x.CampaignId,
                    CampaignHouseholdId = x.CampaignHouseholdId,
                    HouseholdCode = x.CampaignHousehold?.HouseholdCode ?? string.Empty,
                    HeadOfHouseholdName = x.CampaignHousehold?.HeadOfHouseholdName ?? string.Empty,
                    CampaignTeamId = x.CampaignTeamId,
                    CampaignTeamName = x.CampaignTeam?.Team?.Name,
                    DistributionPointId = x.DistributionPointId,
                    DistributionPointName = x.DistributionPoint?.Name,
                    ReliefPackageDefinitionId = x.ReliefPackageDefinitionId,
                    ReliefPackageDefinitionName = x.ReliefPackageDefinition?.Name ?? string.Empty,
                    DeliveryMode = x.DeliveryMode,
                    Status = x.Status,
                    ScheduledAt = x.ScheduledAt,
                    DeliveredAt = x.DeliveredAt,
                    Notes = x.Notes,
                    ProofCount = x.Proofs?.Count ?? 0
                })
                .ToList();

            return new Pagination<HouseholdChecklistItemResponse>(items, totalCount, pageIndex, pageSize);
        }

        public async Task<Pagination<TeamDeliveryWorklistItemResponse>> GetTeamDeliveryWorklistAsync(
            Guid campaignId,
            TeamDeliveryWorklistQueryRequest request,
            CancellationToken cancellationToken = default)
        {
            await EnsureReliefCampaignAsync(campaignId, cancellationToken);

            IQueryable<HouseholdDelivery> query = _unitOfWork.HouseholdDeliveries.GetQueryable()
                .Include(x => x.CampaignHousehold)
                .Include(x => x.CampaignTeam)
                    .ThenInclude(ct => ct.Team)
                .Include(x => x.DistributionPoint)
                .Where(x => x.CampaignId == campaignId);

            query = ApplyDeliveryFilters(query, request);

            if (request.IncludePendingOnly.GetValueOrDefault(true))
                query = query.Where(x => x.Status != HouseholdFulfillmentStatus.Delivered);

            if (request.RequiresBoat.HasValue)
                query = query.Where(x => (x.CampaignHousehold != null && x.CampaignHousehold.RequiresBoat == request.RequiresBoat.Value));

            if (request.RequiresLocalGuide.HasValue)
                query = query.Where(x => (x.CampaignHousehold != null && x.CampaignHousehold.RequiresLocalGuide == request.RequiresLocalGuide.Value));

            if (request.MinFloodSeverityLevel.HasValue)
                query = query.Where(x => (x.CampaignHousehold != null && (x.CampaignHousehold.FloodSeverityLevel ?? 0) >= request.MinFloodSeverityLevel.Value));

            if (request.MinIsolationSeverityLevel.HasValue)
                query = query.Where(x => (x.CampaignHousehold != null && (x.CampaignHousehold.IsolationSeverityLevel ?? 0) >= request.MinIsolationSeverityLevel.Value));

            if (request.PrioritizeIsolated.GetValueOrDefault())
            {
                query = query
                    .OrderByDescending(x => x.CampaignHousehold != null && x.CampaignHousehold.IsIsolated)
                    .ThenByDescending(x => x.CampaignHousehold != null ? (x.CampaignHousehold.IsolationSeverityLevel ?? 0) : 0)
                    .ThenByDescending(x => x.CampaignHousehold != null ? (x.CampaignHousehold.FloodSeverityLevel ?? 0) : 0)
                    .ThenBy(x => x.ScheduledAt);
            }
            else
            {
                query = query.OrderBy(x => x.ScheduledAt);
            }

            var pageIndex = request.PageIndex <= 0 ? 1 : request.PageIndex;
            var pageSize = request.PageSize <= 0 ? 10 : request.PageSize;
            var paged = await Pagination<HouseholdDelivery>.ToPagedList(query, pageIndex, pageSize);
            var items = paged.Items!.Select(MapTeamDeliveryWorklistItem).ToList();

            return new Pagination<TeamDeliveryWorklistItemResponse>(items, paged.TotalCount, paged.CurrentPage, paged.PageSize);
        }

        public async Task<DistributionPointResponse> CreateDistributionPointAsync(
            Guid campaignId,
            CreateDistributionPointRequest request,
            CancellationToken cancellationToken = default)
        {
            var campaign = await EnsureReliefCampaignAsync(campaignId, cancellationToken);

            var stationAttached = campaign.CampaignStations.Any(x => x.ReliefStationId == request.ReliefStationId && x.IsActive);
            if (!stationAttached)
                throw new InvalidOperationException("Relief station is not attached to this campaign.");

            if (request.CampaignTeamId.HasValue)
            {
                var teams = await _unitOfWork.Campaigns.GetCampaignTeamsAsync(campaignId, cancellationToken);
                if (!teams.Any(t => t.CampaignTeamId == request.CampaignTeamId.Value))
                    throw new KeyNotFoundException($"Campaign team '{request.CampaignTeamId}' was not found in this campaign.");
            }

            var entity = new DistributionPoint
            {
                DistributionPointId = Guid.NewGuid(),
                CampaignId = campaignId,
                ReliefStationId = request.ReliefStationId,
                CampaignTeamId = request.CampaignTeamId,
                LocationId = request.LocationId,
                Name = request.Name.Trim(),
                Address = request.Address?.Trim(),
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                DeliveryMode = request.DeliveryMode,
                StartsAt = request.StartsAt,
                EndsAt = request.EndsAt,
                IsActive = request.IsActive
            };

            await _unitOfWork.DistributionPoints.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return MapDistributionPoint(entity);
        }

        public async Task<Pagination<DistributionPointResponse>> GetDistributionPointsAsync(
            Guid campaignId,
            DistributionPointQueryRequest request,
            CancellationToken cancellationToken = default)
        {
            await EnsureReliefCampaignAsync(campaignId, cancellationToken);
            var query = _unitOfWork.DistributionPoints.GetQueryable()
                .Where(x => x.CampaignId == campaignId);

            if (request.ReliefStationId.HasValue)
                query = query.Where(x => x.ReliefStationId == request.ReliefStationId.Value);

            if (request.CampaignTeamId.HasValue)
            {
                var teamId = request.CampaignTeamId.Value;
                query = query.Where(x =>
                    x.CampaignTeamId == teamId ||
                    x.Households.Any(h => h.CampaignTeamId == teamId) ||
                    x.Deliveries.Any(d => d.CampaignTeamId == teamId) ||
                    _unitOfWork.SupplyShortageRequests.GetQueryable().Any(s => s.DistributionPointId == x.DistributionPointId && s.CampaignTeamId == teamId));
            }

            if (request.IsActive.HasValue)
                query = query.Where(x => x.IsActive == request.IsActive.Value);

            if (request.DeliveryMode.HasValue)
                query = query.Where(x => x.DeliveryMode == request.DeliveryMode.Value);

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var keyword = request.Search.Trim();
                query = query.Where(x =>
                    x.Name.Contains(keyword) ||
                    (x.Address ?? string.Empty).Contains(keyword));
            }

            query = query.OrderByDescending(x => x.StartsAt);
            var paged = await Pagination<DistributionPoint>.ToPagedList(query, request.PageIndex, request.PageSize);
            var mapped = paged.Items!.Select(MapDistributionPoint).ToList();
            return new Pagination<DistributionPointResponse>(mapped, paged.TotalCount, paged.CurrentPage, paged.PageSize);
        }

        public async Task<DistributionPointResponse> UpdateDistributionPointAsync(
            Guid campaignId,
            Guid distributionPointId,
            UpdateDistributionPointRequest request,
            CancellationToken cancellationToken = default)
        {
            var campaign = await EnsureReliefCampaignAsync(campaignId, cancellationToken);
            var point = await _unitOfWork.DistributionPoints.GetByIdWithDeliveriesAsync(distributionPointId, cancellationToken)
                ?? throw new KeyNotFoundException($"Distribution point '{distributionPointId}' was not found.");

            if (point.CampaignId != campaignId)
                throw new InvalidOperationException("Distribution point does not belong to campaign.");

            if (!string.IsNullOrWhiteSpace(request.Name))
                point.Name = request.Name.Trim();

            if (request.ReliefStationId.HasValue)
            {
                ValidateStationAttachedToCampaign(campaign, request.ReliefStationId.Value);
                point.ReliefStationId = request.ReliefStationId.Value;
            }

            if (request.CampaignTeamId.HasValue)
            {
                var teams = await _unitOfWork.Campaigns.GetCampaignTeamsAsync(campaignId, cancellationToken);
                if (!teams.Any(t => t.CampaignTeamId == request.CampaignTeamId.Value))
                    throw new KeyNotFoundException($"Campaign team '{request.CampaignTeamId.Value}' was not found in this campaign.");
                point.CampaignTeamId = request.CampaignTeamId;
            }

            if (request.LocationId.HasValue)
                point.LocationId = request.LocationId;

            if (request.Address is not null)
                point.Address = string.IsNullOrWhiteSpace(request.Address) ? null : request.Address.Trim();

            if (request.Latitude.HasValue)
                point.Latitude = request.Latitude.Value;

            if (request.Longitude.HasValue)
                point.Longitude = request.Longitude.Value;

            if (request.DeliveryMode.HasValue)
                point.DeliveryMode = request.DeliveryMode.Value;

            if (request.StartsAt.HasValue)
                point.StartsAt = request.StartsAt.Value;

            if (request.EndsAt.HasValue)
                point.EndsAt = request.EndsAt.Value;

            if (request.IsActive.HasValue)
                point.IsActive = request.IsActive.Value;

            await _unitOfWork.DistributionPoints.UpdateAsync(point);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return MapDistributionPoint(point);
        }

        public async Task<DistributionPointResponse> DeleteDistributionPointAsync(
            Guid campaignId,
            Guid distributionPointId,
            CancellationToken cancellationToken = default)
        {
            await EnsureReliefCampaignAsync(campaignId, cancellationToken);
            var point = await _unitOfWork.DistributionPoints.GetByIdWithDeliveriesAsync(distributionPointId, cancellationToken)
                ?? throw new KeyNotFoundException($"Distribution point '{distributionPointId}' was not found.");

            if (point.CampaignId != campaignId)
                throw new InvalidOperationException("Distribution point does not belong to campaign.");

            point.IsActive = false;
            await _unitOfWork.DistributionPoints.UpdateAsync(point);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return MapDistributionPoint(point);
        }

        public async Task<ReliefPackageDefinitionResponse> CreateReliefPackageDefinitionAsync(
            Guid campaignId,
            CreateReliefPackageDefinitionRequest request,
            CancellationToken cancellationToken = default)
        {
            await EnsureReliefCampaignAsync(campaignId, cancellationToken);

            SupplyItem outputSupplyItem;
            if (request.OutputSupplyItemId.HasValue && request.OutputSupplyItemId.Value != Guid.Empty)
            {
                outputSupplyItem = await _unitOfWork.SupplyItems.GetByIdAsync(request.OutputSupplyItemId.Value)
                    ?? throw new KeyNotFoundException($"Output supply item '{request.OutputSupplyItemId.Value}' was not found.");
            }
            else
            {
                // Auto-create an output supply item representing this package
                outputSupplyItem = new SupplyItem
                {
                    SupplyItemId = Guid.NewGuid(),
                    Name = $"Gói: {request.Name.Trim()}",
                    Description = $"Mục hàng đầu ra tự động tạo cho gói hỗ trợ '{request.Name.Trim()}'",
                    Category = SupplyCategory.Khac,
                    Unit = "gói",
                    CreatedAt = DateTime.UtcNow
                };
                await _unitOfWork.SupplyItems.AddAsync(outputSupplyItem);
            }

            var duplicateSupplyItems = request.Items.GroupBy(x => x.SupplyItemId).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
            if (duplicateSupplyItems.Count != 0)
                throw new InvalidOperationException($"Duplicate supply items in package: {string.Join(", ", duplicateSupplyItems)}");

            var existingDefinitions = await _unitOfWork.ReliefPackageDefinitions.GetByCampaignAsync(campaignId, cancellationToken);
            var outputSupplyItemId = outputSupplyItem.SupplyItemId;
            var packageCategorySupplyItemIds = existingDefinitions
                .Select(x => x.OutputSupplyItemId)
                .ToHashSet();
            packageCategorySupplyItemIds.Add(outputSupplyItemId);

            foreach (var item in request.Items)
            {
                if (!await _unitOfWork.SupplyItems.ExistsAsync(item.SupplyItemId))
                    throw new KeyNotFoundException($"Supply item '{item.SupplyItemId}' was not found.");

                if (item.SupplyItemId == outputSupplyItemId)
                    throw new InvalidOperationException("Output supply item cannot be used as a package component.");

                if (packageCategorySupplyItemIds.Contains(item.SupplyItemId))
                    throw new InvalidOperationException(
                        $"Supply item '{item.SupplyItemId}' is a package-category item and cannot be selected as a package component.");
            }

            if (request.IsDefault)
            {
                foreach (var pkg in existingDefinitions.Where(x => x.IsDefault))
                {
                    pkg.IsDefault = false;
                    await _unitOfWork.ReliefPackageDefinitions.UpdateAsync(pkg);
                }
            }

            var package = new ReliefPackageDefinition
            {
                ReliefPackageDefinitionId = Guid.NewGuid(),
                CampaignId = campaignId,
                OutputSupplyItemId = outputSupplyItem.SupplyItemId,
                Name = request.Name.Trim(),
                Description = request.Description?.Trim(),
                CashSupportAmount = request.CashSupportAmount ?? 0,
                IsDefault = request.IsDefault,
                IsActive = request.IsActive,
                CreatedAt = DateTime.UtcNow,
                Items = request.Items.Select(i => new ReliefPackageDefinitionItem
                {
                    ReliefPackageDefinitionItemId = Guid.NewGuid(),
                    SupplyItemId = i.SupplyItemId,
                    Quantity = i.Quantity,
                    Unit = i.Unit.Trim()
                }).ToList()
            };

            await _unitOfWork.ReliefPackageDefinitions.AddAsync(package);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var saved = await _unitOfWork.ReliefPackageDefinitions.GetByIdWithItemsAsync(package.ReliefPackageDefinitionId, cancellationToken)
                ?? throw new KeyNotFoundException("Relief package was not found after save.");
            return MapPackage(saved);
        }

        public async Task<ReliefPackageAssemblyAvailabilityResponse> GetPackageAssemblyAvailabilityAsync(
            Guid campaignId,
            Guid reliefPackageDefinitionId,
            Guid reliefStationId,
            Guid inventoryId,
            CancellationToken cancellationToken = default)
        {
            var campaign = await EnsureReliefCampaignAsync(campaignId, cancellationToken);
            ValidateStationAttachedToCampaign(campaign, reliefStationId);

            var package = await _unitOfWork.ReliefPackageDefinitions.GetByIdWithItemsAsync(reliefPackageDefinitionId, cancellationToken)
                ?? throw new KeyNotFoundException($"Relief package definition '{reliefPackageDefinitionId}' was not found.");

            if (package.CampaignId != campaignId)
                throw new InvalidOperationException("Relief package definition does not belong to campaign.");

            if (!package.IsActive)
                throw new InvalidOperationException("Relief package definition is inactive.");

            if (package.Items.Count == 0)
                throw new InvalidOperationException("Relief package definition has no component items.");

            var inventory = await _unitOfWork.Inventories.GetByIdAsync(inventoryId)
                ?? throw new KeyNotFoundException($"Inventory '{inventoryId}' was not found.");

            if (inventory.Status != EntityStatus.Active)
                throw new InvalidOperationException("Inventory is not active.");

            if (inventory.ReliefStationId != reliefStationId)
                throw new InvalidOperationException("Inventory does not belong to the selected relief station.");

            var campaignInventory = await _campaignInventoryService.EnsureCampaignInventoryAsync(campaignId, cancellationToken);
            var campaignStocks = await _unitOfWork.CampaignInventoryStocks.GetByCampaignInventoryIdAsync(campaignInventory.CampaignInventoryId, cancellationToken);

            var stockBySupplyItem = campaignStocks
                .ToDictionary(x => x.SupplyItemId, x => x.CurrentQuantity);

            var components = package.Items.Select(item =>
            {
                var available = stockBySupplyItem.TryGetValue(item.SupplyItemId, out var qty) ? qty : 0;
                var maxByItem = item.Quantity > 0 ? available / item.Quantity : 0;

                return new ReliefPackageAssemblyAvailabilityItemResponse
                {
                    SupplyItemId = item.SupplyItemId,
                    SupplyItemName = item.SupplyItem?.Name ?? string.Empty,
                    Unit = item.Unit,
                    RequiredPerPackage = item.Quantity,
                    AvailableQuantity = available,
                    MaxAssemblableByItem = maxByItem
                };
            }).ToList();

            var maxAssemblable = components.Count == 0 ? 0 : components.Min(x => x.MaxAssemblableByItem);

            return new ReliefPackageAssemblyAvailabilityResponse
            {
                CampaignId = campaignId,
                CampaignInventoryId = campaignInventory.CampaignInventoryId,
                ReliefStationId = reliefStationId,
                InventoryId = inventoryId,
                ReliefPackageDefinitionId = reliefPackageDefinitionId,
                OutputSupplyItemId = package.OutputSupplyItemId,
                OutputSupplyItemName = package.OutputSupplyItem?.Name ?? string.Empty,
                OutputUnit = package.OutputSupplyItem?.Unit ?? string.Empty,
                MaxAssemblableQuantity = maxAssemblable,
                Components = components
            };
        }

        public async Task<ReliefPackageAssemblyResponse> AssembleReliefPackageAsync(
            Guid campaignId,
            Guid reliefPackageDefinitionId,
            AssembleReliefPackageRequest request,
            CancellationToken cancellationToken = default)
        {
            var currentUserId = _currentUser.UserId
                ?? throw new UnauthorizedAccessException("User is not authenticated.");

            var availability = await GetPackageAssemblyAvailabilityAsync(
                campaignId,
                reliefPackageDefinitionId,
                request.ReliefStationId,
                request.InventoryId,
                cancellationToken);

            if (request.QuantityToAssemble > availability.MaxAssemblableQuantity)
                throw new InvalidOperationException(
                    $"Insufficient component stock. Maximum assemblable quantity is {availability.MaxAssemblableQuantity}.");

            var package = await _unitOfWork.ReliefPackageDefinitions.GetByIdWithItemsAsync(reliefPackageDefinitionId, cancellationToken)
                ?? throw new KeyNotFoundException($"Relief package definition '{reliefPackageDefinitionId}' was not found.");

            _ = await _campaignInventoryService.EnsureCampaignInventoryAsync(campaignId, cancellationToken);

            await _unitOfWork.BeginTransactionAsync(cancellationToken);
            try
            {
                var consumeItems = package.Items.Select(item => new TransactionItemRequest
                {
                    SupplyItemId = item.SupplyItemId,
                    Quantity = item.Quantity * request.QuantityToAssemble,
                    Notes = $"Package assembly consume for definition {package.ReliefPackageDefinitionId}"
                }).ToList();

                await _campaignInventoryService.CreateTransactionAsync(
                    campaignId,
                    TransactionType.Export,
                    TransactionReason.PackageAssemblyConsume,
                    consumeItems,
                    notes: $"Package assembly consume for definition {package.ReliefPackageDefinitionId}",
                    reliefPackageDefinitionId: package.ReliefPackageDefinitionId,
                    autoSave: false,
                    cancellationToken: cancellationToken);

                await _campaignInventoryService.CreateTransactionAsync(
                    campaignId,
                    TransactionType.Import,
                    TransactionReason.PackageAssemblyProduce,
                    [
                        new TransactionItemRequest
                        {
                            SupplyItemId = package.OutputSupplyItemId,
                            Quantity = request.QuantityToAssemble,
                            Notes = $"Produced package output for definition {package.ReliefPackageDefinitionId}"
                        }
                    ],
                    notes: $"Package assembly produce for definition {package.ReliefPackageDefinitionId}",
                    reliefPackageDefinitionId: package.ReliefPackageDefinitionId,
                    autoSave: false,
                    cancellationToken: cancellationToken);

                var assembly = new ReliefPackageAssembly
                {
                    ReliefPackageAssemblyId = Guid.NewGuid(),
                    CampaignId = campaignId,
                    ReliefStationId = request.ReliefStationId,
                    InventoryId = request.InventoryId,
                    ReliefPackageDefinitionId = package.ReliefPackageDefinitionId,
                    OutputSupplyItemId = package.OutputSupplyItemId,
                    QuantityCreated = request.QuantityToAssemble,
                    CreatedBy = currentUserId,
                    CreatedAt = DateTime.UtcNow,
                    Notes = request.Notes?.Trim(),
                    Details = package.Items.Select(item => new ReliefPackageAssemblyDetail
                    {
                        ReliefPackageAssemblyDetailId = Guid.NewGuid(),
                        SupplyItemId = item.SupplyItemId,
                        QuantityConsumed = item.Quantity * request.QuantityToAssemble,
                        Unit = item.Unit
                    }).ToList()
                };

                await _unitOfWork.ReliefPackageAssemblies.AddAsync(assembly);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                var saved = await _unitOfWork.ReliefPackageAssemblies.GetByIdWithDetailsAsync(assembly.ReliefPackageAssemblyId, cancellationToken)
                    ?? throw new KeyNotFoundException("Relief package assembly was not found after save.");

                return MapPackageAssembly(saved);
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                throw;
            }
        }

        public async Task<IReadOnlyList<ReliefPackageAssemblyResponse>> GetPackageAssemblyHistoryByCampaignAsync(
            Guid campaignId,
            CancellationToken cancellationToken = default)
        {
            await EnsureReliefCampaignAsync(campaignId, cancellationToken);
            var items = await _unitOfWork.ReliefPackageAssemblies.GetByCampaignAsync(campaignId, cancellationToken);
            return items.Select(MapPackageAssembly).ToList();
        }

        public async Task<IReadOnlyList<ReliefPackageAssemblyResponse>> GetPackageAssemblyHistoryByStationAsync(
            Guid campaignId,
            Guid reliefStationId,
            CancellationToken cancellationToken = default)
        {
            var campaign = await EnsureReliefCampaignAsync(campaignId, cancellationToken);
            ValidateStationAttachedToCampaign(campaign, reliefStationId);

            var items = await _unitOfWork.ReliefPackageAssemblies.GetByStationAsync(campaignId, reliefStationId, cancellationToken);
            return items.Select(MapPackageAssembly).ToList();
        }

        public async Task<IReadOnlyList<ReliefPackageAssemblyResponse>> GetPackageAssemblyHistoryByDefinitionAsync(
            Guid campaignId,
            Guid reliefPackageDefinitionId,
            CancellationToken cancellationToken = default)
        {
            await EnsureReliefCampaignAsync(campaignId, cancellationToken);

            var definition = await _unitOfWork.ReliefPackageDefinitions.GetByIdAsync(reliefPackageDefinitionId)
                ?? throw new KeyNotFoundException($"Relief package definition '{reliefPackageDefinitionId}' was not found.");

            if (definition.CampaignId != campaignId)
                throw new InvalidOperationException("Relief package definition does not belong to campaign.");

            var items = await _unitOfWork.ReliefPackageAssemblies.GetByPackageDefinitionAsync(campaignId, reliefPackageDefinitionId, cancellationToken);
            return items.Select(MapPackageAssembly).ToList();
        }

        public async Task<Pagination<ReliefPackageDefinitionResponse>> GetReliefPackageDefinitionsAsync(
            Guid campaignId,
            ReliefPackageQueryRequest request,
            CancellationToken cancellationToken = default)
        {
            await EnsureReliefCampaignAsync(campaignId, cancellationToken);
            var query = _unitOfWork.ReliefPackageDefinitions.GetQueryable()
                .Where(x => x.CampaignId == campaignId);

            if (request.IsActive.HasValue)
                query = query.Where(x => x.IsActive == request.IsActive.Value);

            if (request.IsDefault.HasValue)
                query = query.Where(x => x.IsDefault == request.IsDefault.Value);

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var keyword = request.Search.Trim();
                query = query.Where(x =>
                    x.Name.Contains(keyword) ||
                    (x.Description ?? string.Empty).Contains(keyword) ||
                    (x.OutputSupplyItem.Name ?? string.Empty).Contains(keyword));
            }

            query = query.OrderByDescending(x => x.IsDefault).ThenBy(x => x.Name);
            var paged = await Pagination<ReliefPackageDefinition>.ToPagedList(query, request.PageIndex, request.PageSize);
            var mapped = paged.Items!.Select(MapPackage).ToList();
            return new Pagination<ReliefPackageDefinitionResponse>(mapped, paged.TotalCount, paged.CurrentPage, paged.PageSize);
        }

        public async Task<ReliefPackageDefinitionResponse> UpdateReliefPackageDefinitionAsync(
            Guid campaignId,
            Guid reliefPackageDefinitionId,
            UpdateReliefPackageDefinitionRequest request,
            CancellationToken cancellationToken = default)
        {
            await EnsureReliefCampaignAsync(campaignId, cancellationToken);

            var package = await _unitOfWork.ReliefPackageDefinitions.GetByIdWithItemsAsync(reliefPackageDefinitionId, cancellationToken)
                ?? throw new KeyNotFoundException($"Relief package definition '{reliefPackageDefinitionId}' was not found.");

            if (package.CampaignId != campaignId)
                throw new InvalidOperationException("Relief package definition does not belong to campaign.");

            if (!string.IsNullOrWhiteSpace(request.Name))
                package.Name = request.Name.Trim();

            if (request.Description is not null)
                package.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();

            if (request.CashSupportAmount.HasValue)
            {
                if (request.CashSupportAmount.Value < 0)
                    throw new InvalidOperationException("CashSupportAmount cannot be negative.");

                package.CashSupportAmount = request.CashSupportAmount.Value;
            }

            if (request.OutputSupplyItemId.HasValue)
            {
                if (request.OutputSupplyItemId.Value == Guid.Empty)
                    throw new InvalidOperationException("OutputSupplyItemId cannot be an empty GUID.");

                var outputSupplyItem = await _unitOfWork.SupplyItems.GetByIdAsync(request.OutputSupplyItemId.Value)
                    ?? throw new KeyNotFoundException($"Output supply item '{request.OutputSupplyItemId.Value}' was not found.");
                package.OutputSupplyItemId = outputSupplyItem.SupplyItemId;
            }

            if (request.IsDefault.HasValue)
            {
                package.IsDefault = request.IsDefault.Value;
                if (request.IsDefault.Value)
                {
                    var existingDefinitions = await _unitOfWork.ReliefPackageDefinitions.GetByCampaignAsync(campaignId, cancellationToken);
                    foreach (var existing in existingDefinitions.Where(x => x.ReliefPackageDefinitionId != package.ReliefPackageDefinitionId && x.IsDefault))
                    {
                        existing.IsDefault = false;
                        await _unitOfWork.ReliefPackageDefinitions.UpdateAsync(existing);
                    }
                }
            }

            if (request.IsActive.HasValue)
                package.IsActive = request.IsActive.Value;

            if (request.Items is not null)
            {
                if (request.Items.Count == 0)
                    throw new InvalidOperationException("Relief package definition must contain at least one component item.");

                var duplicateSupplyItems = request.Items.GroupBy(x => x.SupplyItemId).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
                if (duplicateSupplyItems.Count != 0)
                    throw new InvalidOperationException($"Duplicate supply items in package: {string.Join(", ", duplicateSupplyItems)}");

                var outputSupplyItemId = request.OutputSupplyItemId ?? package.OutputSupplyItemId;
                foreach (var item in request.Items)
                {
                    if (!await _unitOfWork.SupplyItems.ExistsAsync(item.SupplyItemId))
                        throw new KeyNotFoundException($"Supply item '{item.SupplyItemId}' was not found.");

                    if (item.SupplyItemId == outputSupplyItemId)
                        throw new InvalidOperationException("Output supply item cannot be used as a package component.");
                }

                foreach (var existingItem in package.Items.ToList())
                {
                    await _unitOfWork.ReliefPackageDefinitionItems.DeleteAsync(existingItem);
                }

                package.Items = request.Items.Select(i => new ReliefPackageDefinitionItem
                {
                    ReliefPackageDefinitionItemId = Guid.NewGuid(),
                    ReliefPackageDefinitionId = package.ReliefPackageDefinitionId,
                    SupplyItemId = i.SupplyItemId,
                    Quantity = i.Quantity,
                    Unit = i.Unit.Trim()
                }).ToList();
            }

            await _unitOfWork.ReliefPackageDefinitions.UpdateAsync(package);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var saved = await _unitOfWork.ReliefPackageDefinitions.GetByIdWithItemsAsync(reliefPackageDefinitionId, cancellationToken)
                ?? throw new KeyNotFoundException("Relief package definition was not found after update.");
            return MapPackage(saved);
        }

        public async Task<ReliefPackageDefinitionResponse> DeleteReliefPackageDefinitionAsync(
            Guid campaignId,
            Guid reliefPackageDefinitionId,
            CancellationToken cancellationToken = default)
        {
            await EnsureReliefCampaignAsync(campaignId, cancellationToken);

            var package = await _unitOfWork.ReliefPackageDefinitions.GetByIdWithItemsAsync(reliefPackageDefinitionId, cancellationToken)
                ?? throw new KeyNotFoundException($"Relief package definition '{reliefPackageDefinitionId}' was not found.");

            if (package.CampaignId != campaignId)
                throw new InvalidOperationException("Relief package definition does not belong to campaign.");

            package.IsActive = false;
            package.IsDefault = false;
            await _unitOfWork.ReliefPackageDefinitions.UpdateAsync(package);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return MapPackage(package);
        }

        public async Task<HouseholdDeliveryResponse> CompleteHouseholdDeliveryAsync(
            Guid campaignId,
            Guid householdDeliveryId,
            CompleteHouseholdDeliveryRequest request,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(request.ProofFileUrl))
                throw new InvalidOperationException("Proof file URL is required for delivered status.");

            return await CompleteHouseholdDeliveryInternalAsync(
                campaignId,
                householdDeliveryId,
                request.ReliefPackageDefinitionId,
                request.CampaignTeamId,
                request.CashSupportAmount,
                request.Notes,
                [new CompleteHouseholdDeliveryProofRequest
                {
                    FileUrl = request.ProofFileUrl,
                    FileType = request.ProofContentType,
                    Note = request.ProofNote
                }],
                cancellationToken);
        }

        public async Task<Pagination<HouseholdDeliveryResponse>> GetDeliveriesAsync(
            Guid campaignId,
            DeliveryQueryRequest request,
            CancellationToken cancellationToken = default)
        {
            await EnsureReliefCampaignAsync(campaignId, cancellationToken);

            IQueryable<HouseholdDelivery> query = _unitOfWork.HouseholdDeliveries.GetQueryable()
                .Where(x => x.CampaignId == campaignId);

            query = ApplyDeliveryFilters(query, request);

            var pageIndex = request.PageIndex <= 0 ? 1 : request.PageIndex;
            var pageSize = request.PageSize <= 0 ? 10 : request.PageSize;
            query = query.OrderByDescending(x => x.ScheduledAt);
            var paged = await Pagination<HouseholdDelivery>.ToPagedList(query, pageIndex, pageSize);
            var mapped = paged.Items!.Select(MapHouseholdDelivery).ToList();

            return new Pagination<HouseholdDeliveryResponse>(mapped, paged.TotalCount, paged.CurrentPage, paged.PageSize);
        }

        public async Task<HouseholdDeliveryResponse> GetDeliveryByIdAsync(
            Guid campaignId,
            Guid householdDeliveryId,
            CancellationToken cancellationToken = default)
        {
            await EnsureReliefCampaignAsync(campaignId, cancellationToken);
            var delivery = await _unitOfWork.HouseholdDeliveries.GetByIdWithProofsAsync(householdDeliveryId, cancellationToken)
                ?? throw new KeyNotFoundException($"Household delivery '{householdDeliveryId}' was not found.");

            if (delivery.CampaignId != campaignId)
                throw new InvalidOperationException("Delivery does not belong to campaign.");

            return MapHouseholdDelivery(delivery);
        }

        public async Task<HouseholdDeliveryResponse> UpdateHouseholdDeliveryAssignmentAsync(
            Guid campaignId,
            Guid householdDeliveryId,
            UpdateHouseholdDeliveryAssignmentRequest request,
            CancellationToken cancellationToken = default)
        {
            await EnsureReliefCampaignAsync(campaignId, cancellationToken);

            var delivery = await _unitOfWork.HouseholdDeliveries.GetByIdWithProofsAsync(householdDeliveryId, cancellationToken)
                ?? throw new KeyNotFoundException($"Household delivery '{householdDeliveryId}' was not found.");

            if (delivery.CampaignId != campaignId)
                throw new InvalidOperationException("Delivery does not belong to campaign.");

            if (delivery.Status == HouseholdFulfillmentStatus.Delivered)
                throw new InvalidOperationException("Delivered assignment cannot be edited.");

            var household = delivery.CampaignHousehold
                ?? throw new KeyNotFoundException("Campaign household was not found for this delivery.");

            if (!household.IsIsolated && request.DeliveryMode == DeliveryMode.DoorToDoor)
                throw new InvalidOperationException("Direct delivery is only allowed for isolated households.");

            if (request.DeliveryMode == DeliveryMode.PickupAtPoint && !request.DistributionPointId.HasValue)
                throw new InvalidOperationException("Pickup delivery requires a distribution point.");

            if (request.DeliveryMode == DeliveryMode.DoorToDoor && !request.CampaignTeamId.HasValue)
                throw new InvalidOperationException("Direct delivery requires a campaign team.");

            if (request.DistributionPointId.HasValue)
            {
                var point = await _unitOfWork.DistributionPoints.GetByIdAsync(request.DistributionPointId.Value)
                    ?? throw new KeyNotFoundException($"Distribution point '{request.DistributionPointId.Value}' was not found.");
                if (point.CampaignId != campaignId)
                    throw new InvalidOperationException("Distribution point does not belong to campaign.");
            }

            if (request.CampaignTeamId.HasValue)
            {
                var teams = await _unitOfWork.Campaigns.GetCampaignTeamsAsync(campaignId, cancellationToken);
                if (!teams.Any(t => t.CampaignTeamId == request.CampaignTeamId.Value))
                    throw new KeyNotFoundException($"Campaign team '{request.CampaignTeamId.Value}' was not found in this campaign.");
            }

            var packageId = request.ReliefPackageDefinitionId ?? delivery.ReliefPackageDefinitionId;
            var package = await _unitOfWork.ReliefPackageDefinitions.GetByIdAsync(packageId)
                ?? throw new KeyNotFoundException($"Relief package definition '{packageId}' was not found.");
            if (package.CampaignId != campaignId)
                throw new InvalidOperationException("Relief package does not belong to campaign.");
            if (!package.IsActive)
                throw new InvalidOperationException("Relief package is inactive and cannot be assigned.");

            delivery.DeliveryMode = request.DeliveryMode;
            delivery.DistributionPointId = request.DeliveryMode == DeliveryMode.PickupAtPoint
                ? request.DistributionPointId
                : null;
            delivery.CampaignTeamId = request.CampaignTeamId;
            delivery.ReliefPackageDefinitionId = packageId;
            delivery.CashSupportAmount = package.CashSupportAmount ?? 0;
            delivery.ScheduledAt = request.ScheduledAt ?? delivery.ScheduledAt;
            delivery.Notes = request.Notes;
            delivery.Status = HouseholdFulfillmentStatus.Pending;

            household.DeliveryMode = request.DeliveryMode;
            household.DistributionPointId = delivery.DistributionPointId;
            household.CampaignTeamId = request.CampaignTeamId;
            household.Notes = request.Notes;
            household.FulfillmentStatus = HouseholdFulfillmentStatus.Pending;

            await _unitOfWork.HouseholdDeliveries.UpdateAsync(delivery);
            await _unitOfWork.CampaignHouseholds.UpdateAsync(household);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var saved = await _unitOfWork.HouseholdDeliveries.GetByIdWithProofsAsync(householdDeliveryId, cancellationToken)
                ?? throw new KeyNotFoundException("Updated delivery was not found after save.");
            return MapHouseholdDelivery(saved);
        }

        public async Task DeleteHouseholdDeliveryAssignmentAsync(
            Guid campaignId,
            Guid householdDeliveryId,
            CancellationToken cancellationToken = default)
        {
            await EnsureReliefCampaignAsync(campaignId, cancellationToken);

            var delivery = await _unitOfWork.HouseholdDeliveries.GetQueryable()
                .Include(x => x.CampaignHousehold)
                .Include(x => x.MemberTaskDeliveries)
                .Include(x => x.Proofs)
                .FirstOrDefaultAsync(x => x.HouseholdDeliveryId == householdDeliveryId, cancellationToken)
                ?? throw new KeyNotFoundException($"Household delivery '{householdDeliveryId}' was not found.");

            if (delivery.CampaignId != campaignId)
                throw new InvalidOperationException("Delivery does not belong to campaign.");

            if (delivery.Status == HouseholdFulfillmentStatus.Delivered)
                throw new InvalidOperationException("Delivered assignment cannot be deleted.");

            if (delivery.Proofs.Any())
                throw new InvalidOperationException("Assignment with proofs cannot be deleted.");

            if (delivery.MemberTaskDeliveries.Any())
                throw new InvalidOperationException("Assignment already linked to member tasks and cannot be deleted.");

            if (delivery.CampaignHousehold is not null)
            {
                delivery.CampaignHousehold.CampaignTeamId = null;
                delivery.CampaignHousehold.DistributionPointId = null;
                delivery.CampaignHousehold.Notes = null;
                delivery.CampaignHousehold.FulfillmentStatus = HouseholdFulfillmentStatus.Pending;
                await _unitOfWork.CampaignHouseholds.UpdateAsync(delivery.CampaignHousehold);
            }

            await _unitOfWork.HouseholdDeliveries.DeleteAsync(delivery);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task<BatchCompleteHouseholdDeliveryResponse> CompleteHouseholdDeliveriesBatchAsync(
            Guid campaignId,
            CompleteHouseholdDeliveryBatchRequest request,
            CancellationToken cancellationToken = default)
        {
            await EnsureReliefCampaignAsync(campaignId, cancellationToken);

            var response = new BatchCompleteHouseholdDeliveryResponse
            {
                TotalRequested = request.Items.Count
            };

            foreach (var item in request.Items)
            {
                try
                {
                    var delivery = await CompleteHouseholdDeliveryInternalAsync(
                        campaignId,
                        item.HouseholdDeliveryId,
                        item.ReliefPackageDefinitionId,
                        item.CampaignTeamId,
                        item.CashSupportAmount,
                        item.Notes,
                        item.Proofs,
                        cancellationToken);

                    response.Items.Add(new BatchCompleteHouseholdDeliveryItemResponse
                    {
                        HouseholdDeliveryId = item.HouseholdDeliveryId,
                        IsSuccess = true,
                        Delivery = delivery
                    });
                }
                catch (Exception ex)
                {
                    response.Items.Add(new BatchCompleteHouseholdDeliveryItemResponse
                    {
                        HouseholdDeliveryId = item.HouseholdDeliveryId,
                        IsSuccess = false,
                        Error = ex.Message
                    });
                }
            }

            response.SuccessCount = response.Items.Count(x => x.IsSuccess);
            response.FailureCount = response.Items.Count - response.SuccessCount;
            return response;
        }

        public async Task<SupplyShortageRequestResponse> CreateShortageRequestAsync(
            Guid campaignId,
            CreateSupplyShortageRequest request,
            CancellationToken cancellationToken = default)
        {
            await EnsureReliefCampaignAsync(campaignId, cancellationToken);
            var requesterUserId = _currentUser.UserId ?? throw new UnauthorizedAccessException("User is not authenticated.");

            if (request.CampaignTeamId.HasValue)
            {
                var teams = await _unitOfWork.Campaigns.GetCampaignTeamsAsync(campaignId, cancellationToken);
                if (!teams.Any(t => t.CampaignTeamId == request.CampaignTeamId.Value))
                    throw new KeyNotFoundException($"Campaign team '{request.CampaignTeamId}' was not found in this campaign.");
            }

            if (request.DistributionPointId.HasValue)
            {
                var point = await _unitOfWork.DistributionPoints.GetByIdAsync(request.DistributionPointId.Value)
                    ?? throw new KeyNotFoundException($"Distribution point '{request.DistributionPointId}' was not found.");
                if (point.CampaignId != campaignId)
                    throw new InvalidOperationException("Distribution point does not belong to campaign.");
            }

            var duplicates = request.Items.GroupBy(x => x.SupplyItemId).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
            if (duplicates.Count != 0)
                throw new InvalidOperationException($"Duplicate supply items in shortage request: {string.Join(", ", duplicates)}");

            foreach (var item in request.Items)
            {
                if (!await _unitOfWork.SupplyItems.ExistsAsync(item.SupplyItemId))
                    throw new KeyNotFoundException($"Supply item '{item.SupplyItemId}' was not found.");
            }

            var entity = new SupplyShortageRequest
            {
                SupplyShortageRequestId = Guid.NewGuid(),
                CampaignId = campaignId,
                DistributionPointId = request.DistributionPointId,
                CampaignTeamId = request.CampaignTeamId,
                RequestedByUserId = requesterUserId,
                Status = SupplyShortageRequestStatus.Pending,
                Reason = request.Reason,
                RequestedAt = DateTime.UtcNow,
                Items = request.Items.Select(i => new SupplyShortageRequestItem
                {
                    SupplyShortageRequestItemId = Guid.NewGuid(),
                    SupplyItemId = i.SupplyItemId,
                    QuantityRequested = i.QuantityRequested,
                    Note = i.Note
                }).ToList()
            };

            await _unitOfWork.SupplyShortageRequests.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var saved = await _unitOfWork.SupplyShortageRequests.GetByIdWithItemsAsync(entity.SupplyShortageRequestId, cancellationToken)
                ?? throw new KeyNotFoundException("Shortage request was not found after save.");
            return MapShortage(saved);
        }

        public async Task<Pagination<SupplyShortageRequestResponse>> GetShortageRequestsAsync(
            Guid campaignId,
            SupplyShortageRequestQueryRequest request,
            CancellationToken cancellationToken = default)
        {
            await EnsureReliefCampaignAsync(campaignId, cancellationToken);

            IQueryable<SupplyShortageRequest> query = _unitOfWork.SupplyShortageRequests.GetQueryable()
                .Where(x => x.CampaignId == campaignId);

            if (request.Status.HasValue)
                query = query.Where(x => x.Status == request.Status.Value);

            if (request.DistributionPointId.HasValue)
                query = query.Where(x => x.DistributionPointId == request.DistributionPointId.Value);

            if (request.CampaignTeamId.HasValue)
                query = query.Where(x => x.CampaignTeamId == request.CampaignTeamId.Value);

            if (request.RequestedByUserId.HasValue)
                query = query.Where(x => x.RequestedByUserId == request.RequestedByUserId.Value);

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var keyword = request.Search.Trim();
                query = query.Where(x =>
                    (x.Reason ?? string.Empty).Contains(keyword) ||
                    (x.DistributionPoint != null && x.DistributionPoint.Name.Contains(keyword)) ||
                    (x.CampaignTeam != null && x.CampaignTeam.Team != null && x.CampaignTeam.Team.Name.Contains(keyword)) ||
                    x.Items.Any(i => (i.SupplyItem != null && i.SupplyItem.Name.Contains(keyword)) || (i.Note ?? string.Empty).Contains(keyword)));
            }

            query = query.OrderByDescending(x => x.RequestedAt);
            var paged = await Pagination<SupplyShortageRequest>.ToPagedList(query, request.PageIndex, request.PageSize);
            var mapped = paged.Items!.Select(MapShortage).ToList();
            return new Pagination<SupplyShortageRequestResponse>(mapped, paged.TotalCount, paged.CurrentPage, paged.PageSize);
        }

        public async Task<SupplyShortageRequestResponse> ApproveShortageRequestAsync(
            Guid campaignId,
            Guid shortageRequestId,
            ReviewSupplyShortageRequest request,
            CancellationToken cancellationToken = default)
        {
            var campaign = await EnsureReliefCampaignAsync(campaignId, cancellationToken);
            var entity = await _unitOfWork.SupplyShortageRequests.GetByIdWithItemsAsync(shortageRequestId, cancellationToken)
                ?? throw new KeyNotFoundException($"Supply shortage request '{shortageRequestId}' was not found.");

            if (entity.CampaignId != campaignId)
                throw new InvalidOperationException("Shortage request does not belong to campaign.");

            if (entity.Status != SupplyShortageRequestStatus.Pending)
                throw new InvalidOperationException("Only pending shortage requests can be approved.");

            var approvedMap = (request.ApprovedItems ?? [])
                .GroupBy(x => x.SupplyItemId)
                .ToDictionary(g => g.Key, g => g.First().QuantityApproved);

            foreach (var item in entity.Items)
            {
                item.QuantityApproved = approvedMap.TryGetValue(item.SupplyItemId, out var approved)
                    ? approved
                    : item.QuantityRequested;
            }

            var approvedItems = entity.Items.Where(i => (i.QuantityApproved ?? 0) > 0).ToList();
            if (approvedItems.Count == 0)
                throw new InvalidOperationException("At least one approved item with quantity > 0 is required.");

            var stationId = campaign.CampaignStations.FirstOrDefault(x => x.IsActive)?.ReliefStationId
                ?? throw new InvalidOperationException("Relief campaign has no active station for shortage approval stock movement.");

            var inventory = await _unitOfWork.Inventories.GetActiveByReliefStationAsync(stationId, cancellationToken)
                ?? throw new InvalidOperationException("Active inventory not found for campaign station.");

            // MVP decision: approved shortage immediately books stock movement from same station inventory
            // using existing inventory transaction workflow, instead of introducing extra transfer subsystem.
            await _unitOfWork.BeginTransactionAsync(cancellationToken);
            try
            {
                await _inventoryTransactionService.CreateTransactionAsync(new CreateTransactionRequest
                {
                    InventoryId = inventory.InventoryId,
                    Type = TransactionType.Export,
                    Reason = TransactionReason.CampaignAllocation,
                    Notes = $"Shortage request approved: {entity.SupplyShortageRequestId}",
                    Items = approvedItems.Select(i => new TransactionItemRequest
                    {
                        SupplyItemId = i.SupplyItemId,
                        Quantity = i.QuantityApproved ?? 0,
                        Notes = i.Note
                    }).ToList()
                }, autoSave: false, cancellationToken);

                entity.Status = SupplyShortageRequestStatus.Approved;
                entity.ReviewedAt = DateTime.UtcNow;
                entity.ReviewedByUserId = _currentUser.UserId;
                entity.ReviewNote = request.ReviewNote;
                await _unitOfWork.SupplyShortageRequests.UpdateAsync(entity);

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await _unitOfWork.CommitTransactionAsync(cancellationToken);
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                throw;
            }

            var saved = await _unitOfWork.SupplyShortageRequests.GetByIdWithItemsAsync(shortageRequestId, cancellationToken)
                ?? throw new KeyNotFoundException("Shortage request was not found after approval.");
            return MapShortage(saved);
        }

        public async Task<SupplyShortageRequestResponse> RejectShortageRequestAsync(
            Guid campaignId,
            Guid shortageRequestId,
            ReviewSupplyShortageRequest request,
            CancellationToken cancellationToken = default)
        {
            await EnsureReliefCampaignAsync(campaignId, cancellationToken);

            var entity = await _unitOfWork.SupplyShortageRequests.GetByIdWithItemsAsync(shortageRequestId, cancellationToken)
                ?? throw new KeyNotFoundException($"Supply shortage request '{shortageRequestId}' was not found.");

            if (entity.CampaignId != campaignId)
                throw new InvalidOperationException("Shortage request does not belong to campaign.");

            if (entity.Status != SupplyShortageRequestStatus.Pending)
                throw new InvalidOperationException("Only pending shortage requests can be rejected.");

            entity.Status = SupplyShortageRequestStatus.Rejected;
            entity.ReviewedAt = DateTime.UtcNow;
            entity.ReviewedByUserId = _currentUser.UserId;
            entity.ReviewNote = request.ReviewNote;

            await _unitOfWork.SupplyShortageRequests.UpdateAsync(entity);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var saved = await _unitOfWork.SupplyShortageRequests.GetByIdWithItemsAsync(shortageRequestId, cancellationToken)
                ?? throw new KeyNotFoundException("Shortage request was not found after rejection.");
            return MapShortage(saved);
        }

        private async Task<Campaign> EnsureReliefCampaignAsync(Guid campaignId, CancellationToken cancellationToken)
        {
            var campaign = await _unitOfWork.Campaigns.GetWithDetailsAsync(campaignId, cancellationToken)
                ?? throw new KeyNotFoundException($"Campaign '{campaignId}' was not found.");

            if (campaign.Type != CampaignType.Relief)
                throw new InvalidOperationException("This operation is only available for relief campaigns.");

            return campaign;
        }

        private static List<List<CampaignHousehold>> BuildAreaGroups(IReadOnlyCollection<CampaignHousehold> households)
        {
            var grouped = new List<List<CampaignHousehold>>();

            foreach (var household in households.OrderByDescending(x => x.IsIsolated).ThenByDescending(x => x.HouseholdSize))
            {
                var matchedGroup = grouped.FirstOrDefault(group => BelongsToSameArea(group[0], household));
                if (matchedGroup is null)
                {
                    grouped.Add([household]);
                    continue;
                }

                matchedGroup.Add(household);
            }

            return grouped;
        }

        private static bool BelongsToSameArea(CampaignHousehold seed, CampaignHousehold candidate)
        {
            if (seed.LocationId.HasValue && candidate.LocationId.HasValue && seed.LocationId == candidate.LocationId)
                return true;

            if (HasCoordinates(seed) && HasCoordinates(candidate))
                return CalculateDistanceKm(seed.Latitude, seed.Longitude, candidate.Latitude, candidate.Longitude) <= 1.5d;

            if (!string.IsNullOrWhiteSpace(seed.Address) && !string.IsNullOrWhiteSpace(candidate.Address))
            {
                var seedPrefix = NormalizeAreaAddress(seed.Address);
                var candidatePrefix = NormalizeAreaAddress(candidate.Address);
                return seedPrefix == candidatePrefix;
            }

            return false;
        }

        private static string ResolveAreaName(IReadOnlyCollection<CampaignHousehold> households)
        {
            var sample = households.First();

            if (!string.IsNullOrWhiteSpace(sample.Location?.FullName))
                return sample.Location.FullName;

            if (!string.IsNullOrWhiteSpace(sample.Location?.Name))
                return sample.Location.Name;

            var coordinateGroup = households.Where(HasCoordinates).ToList();
            if (coordinateGroup.Count > 0)
            {
                var centerLat = coordinateGroup.Average(x => x.Latitude);
                var centerLng = coordinateGroup.Average(x => x.Longitude);
                return $"Cụm tọa độ ({centerLat:F4}, {centerLng:F4})";
            }

            if (!string.IsNullOrWhiteSpace(sample.Address))
                return NormalizeAreaAddress(sample.Address);

            return "Chưa phân khu vực";
        }

        private static string NormalizeAreaAddress(string address)
        {
            var normalized = address.Trim();
            var separators = new[] { ',', ';', '-' };
            foreach (var separator in separators)
            {
                var index = normalized.IndexOf(separator);
                if (index > 0)
                {
                    normalized = normalized[..index];
                    break;
                }
            }

            return normalized.Trim();
        }

        private static Location? ResolveLocationContext(IReadOnlyCollection<CampaignHousehold> households, IReadOnlyCollection<Location> locations)
        {
            var addressTokens = households
                .Select(x => x.Address)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .SelectMany(address => address!.Split(new[] { ',', ';', '-' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                .Select(StringHelper.NormalizeVietnamesePath)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct()
                .ToList();

            if (addressTokens.Count() == 0)
                return null;

            return locations
                .OrderByDescending(location => addressTokens.Count(token =>
                    token.Contains(location.NormalizedName, StringComparison.OrdinalIgnoreCase) ||
                    location.NormalizedName.Contains(token, StringComparison.OrdinalIgnoreCase)))
                .ThenByDescending(location => location.Level)
                .FirstOrDefault(location => addressTokens.Any(token =>
                    token.Contains(location.NormalizedName, StringComparison.OrdinalIgnoreCase) ||
                    location.NormalizedName.Contains(token, StringComparison.OrdinalIgnoreCase)));
        }

        private static bool HasCoordinates(CampaignHousehold household)
            => household.Latitude != 0 || household.Longitude != 0;

        private static int CalculateSuggestedTeamCount(
            int householdCount,
            int isolatedHouseholdCount,
            int population,
            decimal populationDensity,
            IReadOnlyCollection<CampaignHousehold> households)
        {
            if (householdCount <= 0)
                return 0;

            var baseTeams = (int)Math.Ceiling(householdCount / 50d);
            var isolationFactor = isolatedHouseholdCount > 0
                ? (int)Math.Ceiling(isolatedHouseholdCount / 12d)
                : 0;
            var populationFactor = population > 0
                ? (int)Math.Ceiling(population / 180d)
                : 0;
            var densityFactor = populationDensity >= 3000
                ? 1
                : populationDensity >= 1000 ? 1 : 0;

            var radiusKm = EstimateCoverageRadiusKm(households);
            var spreadFactor = radiusKm >= 8
                ? 2
                : radiusKm >= 3 ? 1 : 0;

            var floodFactor = households
                .Where(x => x.FloodSeverityLevel.HasValue)
                .Select(x => x.FloodSeverityLevel!.Value)
                .DefaultIfEmpty(0)
                .Average() >= 6 ? 1 : 0;

            return Math.Max(baseTeams, baseTeams + isolationFactor + populationFactor + densityFactor + spreadFactor + floodFactor);
        }

        private static int CalculateEstimatedReliefPersonnel(int suggestedTeamCount, int population)
            => Math.Max(suggestedTeamCount * 4, (int)Math.Ceiling(population / 25d));

        private static int CalculateSuggestedPeoplePerTeam(int suggestedTeamCount, int population)
        {
            if (suggestedTeamCount <= 0)
                return 0;

            return Math.Max(4, (int)Math.Ceiling(population / Math.Max(1d, suggestedTeamCount * 12d)) * 2);
        }

        private static int CalculateSuggestedPeoplePerLine(int householdCount, decimal populationDensity)
        {
            if (householdCount <= 0)
                return 0;

            return populationDensity >= 1000 ? 4 : 2;
        }

        private static int CalculateEstimatedLocalVolunteers(int isolatedHouseholdCount)
            => isolatedHouseholdCount <= 0 ? 0 : Math.Max(1, (int)Math.Ceiling(isolatedHouseholdCount / 10d));

        private static int CalculateEstimatedBoatCount(int isolatedHouseholdCount, double floodSeverityAverage, int requiredBoatCount)
        {
            var isolatedFactor = isolatedHouseholdCount <= 0 ? 0 : (int)Math.Ceiling(isolatedHouseholdCount / 6d);
            var floodFactor = floodSeverityAverage >= 7 ? 2 : floodSeverityAverage >= 4 ? 1 : 0;
            return Math.Max(requiredBoatCount, isolatedFactor + floodFactor);
        }

        private static int CalculateEstimatedLifeJacketCount(int reliefPersonnel, int localVolunteers, int boatCount)
            => reliefPersonnel + localVolunteers + (boatCount * 2);

        private static string GetPriorityLabel(int householdSize)
            => householdSize >= 5 ? "Khẩn cấp" : householdSize >= 3 ? "Ưu tiên cao" : "Ưu tiên";

        private static string GetSuggestedSupportMode(CampaignHousehold household)
        {
            if (household.RequiresBoat || (household.FloodSeverityLevel ?? 0) >= 6)
                return "Đội cơ động đường thủy / giao tận nơi";

            if ((household.IsolationSeverityLevel ?? 0) >= 5 || household.IsIsolated)
                return "Đội cơ động / giao tận nơi";

            return household.DeliveryMode == DeliveryMode.DoorToDoor ? "Giao tận nơi" : "Nhận tại điểm gần nhất";
        }

        private static string GetRecommendedOperationalMode(decimal populationDensity, double estimatedCoverageRadiusKm, int isolatedHouseholdCount)
        {
            if (populationDensity >= 1000 && estimatedCoverageRadiusKm <= 3 && isolatedHouseholdCount <= 3)
                return "Ưu tiên điểm phát";

            if (populationDensity < 1000 && (estimatedCoverageRadiusKm >= 3 || isolatedHouseholdCount > 0))
                return "Ưu tiên đội cơ động";

            return isolatedHouseholdCount >= 4 ? "Ưu tiên đội cơ động" : "Kết hợp điểm phát và đội cơ động";
        }

        private static int CalculateSuggestedDistributionPointCount(int householdCount, decimal populationDensity, double estimatedCoverageRadiusKm, int isolatedHouseholdCount)
        {
            if (householdCount <= 0)
                return 0;

            if (populationDensity >= 1000 && estimatedCoverageRadiusKm <= 3 && isolatedHouseholdCount <= 3)
                return Math.Max(1, (int)Math.Ceiling(householdCount / 35d));

            return 0;
        }

        private static int CalculateSuggestedMobileTeamCount(int suggestedTeamCount, decimal populationDensity, double estimatedCoverageRadiusKm, int isolatedHouseholdCount)
        {
            if (suggestedTeamCount <= 0)
                return 0;

            if (populationDensity < 1000 && estimatedCoverageRadiusKm >= 3)
                return Math.Max(1, suggestedTeamCount);

            if (isolatedHouseholdCount > 0)
                return Math.Max(1, (int)Math.Ceiling(isolatedHouseholdCount / 4d));

            return 0;
        }

        private static string GetTravelComplexityLabel(double estimatedCoverageRadiusKm, decimal populationDensity, int isolatedHouseholdCount)
        {
            if (isolatedHouseholdCount >= 5 || estimatedCoverageRadiusKm >= 8)
                return "Phức tạp cao";

            if (populationDensity >= 1000 || estimatedCoverageRadiusKm >= 3)
                return "Phức tạp vừa";

            return "Phức tạp thấp";
        }

        private static double EstimateCoverageRadiusKm(IReadOnlyCollection<CampaignHousehold> households)
        {
            var validCoordinates = households
                .Where(x => x.Latitude != 0 || x.Longitude != 0)
                .Select(x => new { x.Latitude, x.Longitude })
                .ToList();

            if (validCoordinates.Count <= 1)
                return 0;

            var minLat = validCoordinates.Min(x => x.Latitude);
            var maxLat = validCoordinates.Max(x => x.Latitude);
            var minLng = validCoordinates.Min(x => x.Longitude);
            var maxLng = validCoordinates.Max(x => x.Longitude);

            return CalculateDistanceKm(minLat, minLng, maxLat, maxLng);
        }

        private static double CalculateDistanceKm(double lat1, double lon1, double lat2, double lon2)
        {
            const double earthRadiusKm = 6371;
            var dLat = DegreesToRadians(lat2 - lat1);
            var dLon = DegreesToRadians(lon2 - lon1);

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(DegreesToRadians(lat1)) * Math.Cos(DegreesToRadians(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return earthRadiusKm * c;
        }

        private static double DegreesToRadians(double degrees)
            => degrees * Math.PI / 180d;

        private static IQueryable<HouseholdDelivery> ApplyDeliveryFilters(IQueryable<HouseholdDelivery> query, DeliveryQueryRequest request)
        {
            if (request.CampaignTeamId.HasValue)
                query = query.Where(x => x.CampaignTeamId == request.CampaignTeamId.Value);

            if (request.DistributionPointId.HasValue)
                query = query.Where(x => x.DistributionPointId == request.DistributionPointId.Value);

            if (request.Status.HasValue)
                query = query.Where(x => x.Status == request.Status.Value);

            if (request.DeliveryMode.HasValue)
                query = query.Where(x => x.DeliveryMode == request.DeliveryMode.Value);

            if (request.ScheduledFrom.HasValue)
                query = query.Where(x => x.ScheduledAt >= request.ScheduledFrom.Value);

            if (request.ScheduledTo.HasValue)
                query = query.Where(x => x.ScheduledAt <= request.ScheduledTo.Value);

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var keyword = request.Search.Trim();
                query = query.Where(x =>
                    (x.CampaignHousehold != null && (
                        x.CampaignHousehold.HouseholdCode.Contains(keyword) ||
                        x.CampaignHousehold.HeadOfHouseholdName.Contains(keyword) ||
                        (x.CampaignHousehold.ContactPhone ?? string.Empty).Contains(keyword) ||
                        (x.CampaignHousehold.Address ?? string.Empty).Contains(keyword))) ||
                    (x.Notes ?? string.Empty).Contains(keyword));
            }

            return query;
        }

        private async Task<HouseholdDeliveryResponse> CompleteHouseholdDeliveryInternalAsync(
            Guid campaignId,
            Guid householdDeliveryId,
            Guid? reliefPackageDefinitionId,
            Guid? campaignTeamId,
            decimal? cashSupportAmount,
            string? notes,
            IReadOnlyCollection<CompleteHouseholdDeliveryProofRequest> proofs,
            CancellationToken cancellationToken)
        {
            if (proofs.Count == 0 || proofs.Any(x => string.IsNullOrWhiteSpace(x.FileUrl)))
                throw new InvalidOperationException("At least one valid proof file URL is required for delivered status.");

            var delivery = await _unitOfWork.HouseholdDeliveries.GetByIdWithProofsAsync(householdDeliveryId, cancellationToken)
                ?? throw new KeyNotFoundException($"Household delivery '{householdDeliveryId}' was not found.");

            if (delivery.CampaignId != campaignId)
                throw new InvalidOperationException("Delivery does not belong to campaign.");

            if (delivery.Status == HouseholdFulfillmentStatus.Delivered)
                throw new InvalidOperationException("Delivery already completed.");

            var package = await _unitOfWork.ReliefPackageDefinitions.GetByIdWithItemsAsync(delivery.ReliefPackageDefinitionId, cancellationToken)
                ?? throw new KeyNotFoundException($"Relief package definition '{delivery.ReliefPackageDefinitionId}' was not found.");

            if (reliefPackageDefinitionId.HasValue)
            {
                var pkg = await _unitOfWork.ReliefPackageDefinitions.GetByIdAsync(reliefPackageDefinitionId.Value)
                    ?? throw new KeyNotFoundException($"Relief package definition '{reliefPackageDefinitionId.Value}' was not found.");
                if (pkg.CampaignId != campaignId)
                    throw new InvalidOperationException("Relief package does not belong to campaign.");
                delivery.ReliefPackageDefinitionId = reliefPackageDefinitionId.Value;
                package = await _unitOfWork.ReliefPackageDefinitions.GetByIdWithItemsAsync(reliefPackageDefinitionId.Value, cancellationToken)
                    ?? throw new KeyNotFoundException($"Relief package definition '{reliefPackageDefinitionId.Value}' was not found.");
                delivery.CashSupportAmount = package.CashSupportAmount ?? 0;
            }

            if (campaignTeamId.HasValue)
            {
                var teams = await _unitOfWork.Campaigns.GetCampaignTeamsAsync(campaignId, cancellationToken);
                if (!teams.Any(t => t.CampaignTeamId == campaignTeamId.Value))
                    throw new KeyNotFoundException($"Campaign team '{campaignTeamId.Value}' was not found in this campaign.");
                delivery.CampaignTeamId = campaignTeamId;
            }

            if (cashSupportAmount.HasValue)
            {
                if (cashSupportAmount.Value < 0)
                    throw new InvalidOperationException("CashSupportAmount cannot be negative.");

                delivery.CashSupportAmount = cashSupportAmount.Value;
            }

            await _unitOfWork.BeginTransactionAsync(cancellationToken);
            try
            {
                var campaign = await _unitOfWork.Campaigns.GetWithDetailsAsync(campaignId, cancellationToken)
                    ?? throw new KeyNotFoundException($"Campaign '{campaignId}' was not found.");

                var totalDistributionMoney = delivery.CashSupportAmount;
                var reliefCampaignBalance = campaign.BudgetTotal - campaign.BudgetSpent;

                if (totalDistributionMoney > reliefCampaignBalance)
                {
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    await LogDistributionFailureAsync(delivery, package, totalDistributionMoney, reliefCampaignBalance, cancellationToken);
                    throw new InvalidOperationException("Insufficient balance. Please extract more funds from fundraising campaign or create a new fundraising campaign.");
                }

                var packageItems = package.Items?
                    .Where(x => x.Quantity > 0)
                    .Select(x => new TransactionItemRequest
                    {
                        SupplyItemId = x.SupplyItemId,
                        Quantity = x.Quantity,
                        Notes = $"Household delivery consume package component for delivery {delivery.HouseholdDeliveryId}"
                    })
                    .ToList() ?? [];

                if (packageItems.Count == 0)
                    throw new InvalidOperationException($"Relief package '{package.ReliefPackageDefinitionId}' has no component items to consume.");

                await _campaignInventoryService.CreateTransactionAsync(
                    campaignId,
                    TransactionType.Export,
                    TransactionReason.Other,
                    packageItems,
                    notes: $"Household delivery completion consume package components for delivery {delivery.HouseholdDeliveryId}",
                    campaignTeamId: delivery.CampaignTeamId ?? campaignTeamId,
                    distributionPointId: delivery.DistributionPointId,
                    householdDeliveryId: delivery.HouseholdDeliveryId,
                    reliefPackageDefinitionId: delivery.ReliefPackageDefinitionId,
                    autoSave: false,
                    cancellationToken: cancellationToken);

                campaign.BudgetSpent += totalDistributionMoney;
                await _unitOfWork.Campaigns.UpdateAsync(campaign);

                foreach (var proofItem in proofs)
                {
                    await _unitOfWork.HouseholdDeliveryProofs.AddAsync(new HouseholdDeliveryProof
                    {
                        HouseholdDeliveryProofId = Guid.NewGuid(),
                        HouseholdDeliveryId = delivery.HouseholdDeliveryId,
                        FileUrl = proofItem.FileUrl.Trim(),
                        FileType = proofItem.FileType,
                        Note = proofItem.Note,
                        CapturedAt = DateTime.UtcNow,
                        CapturedByUserId = _currentUser.UserId
                    });
                }

                delivery.Status = HouseholdFulfillmentStatus.Delivered;
                delivery.DeliveredAt = DateTime.UtcNow;
                delivery.DeliveredByUserId = _currentUser.UserId;
                delivery.Notes = notes ?? delivery.Notes;
                await _unitOfWork.HouseholdDeliveries.UpdateAsync(delivery);

                var household = await _unitOfWork.CampaignHouseholds.GetByIdAsync(delivery.CampaignHouseholdId)
                    ?? throw new KeyNotFoundException($"Campaign household '{delivery.CampaignHouseholdId}' was not found.");
                household.FulfillmentStatus = HouseholdFulfillmentStatus.Delivered;
                await _unitOfWork.CampaignHouseholds.UpdateAsync(household);

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await _unitOfWork.CommitTransactionAsync(cancellationToken);
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                throw;
            }

            var saved = await _unitOfWork.HouseholdDeliveries.GetByIdWithProofsAsync(householdDeliveryId, cancellationToken)
                ?? throw new KeyNotFoundException("Household delivery was not found after completion.");
            return MapHouseholdDelivery(saved);
        }

        private static CampaignHouseholdResponse MapCampaignHousehold(CampaignHousehold x) => new()
        {
            CampaignHouseholdId = x.CampaignHouseholdId,
            CampaignId = x.CampaignId,
            LocationId = x.LocationId,
            DistributionPointId = x.DistributionPointId,
            DistributionPointName = x.DistributionPoint?.Name,
            CampaignTeamId = x.CampaignTeamId,
            CampaignTeamName = x.CampaignTeam?.Team?.Name,
            HouseholdCode = x.HouseholdCode,
            HeadOfHouseholdName = x.HeadOfHouseholdName,
            ContactPhone = x.ContactPhone,
            Address = x.Address,
            Latitude = x.Latitude,
            Longitude = x.Longitude,
            HouseholdSize = x.HouseholdSize,
            IsIsolated = x.IsIsolated,
            FloodSeverityLevel = x.FloodSeverityLevel,
            IsolationSeverityLevel = x.IsolationSeverityLevel,
            RequiresBoat = x.RequiresBoat,
            RequiresLocalGuide = x.RequiresLocalGuide,
            DeliveryMode = x.DeliveryMode,
            FulfillmentStatus = x.FulfillmentStatus,
            Notes = x.Notes,
            CreatedAt = x.CreatedAt
        };

        private static DistributionPointResponse MapDistributionPoint(DistributionPoint x) => new()
        {
            DistributionPointId = x.DistributionPointId,
            CampaignId = x.CampaignId,
            ReliefStationId = x.ReliefStationId,
            CampaignTeamId = x.CampaignTeamId,
            CampaignTeamName = x.CampaignTeam?.Team?.Name,
            LocationId = x.LocationId,
            Name = x.Name,
            Address = x.Address,
            Latitude = x.Latitude,
            Longitude = x.Longitude,
            DeliveryMode = x.DeliveryMode,
            StartsAt = x.StartsAt,
            EndsAt = x.EndsAt,
            IsActive = x.IsActive,
            AssignedHouseholdCount = x.Households.Count,
            PendingDeliveryCount = x.Deliveries.Count(d => d.Status != HouseholdFulfillmentStatus.Delivered),
            TotalDeliveryCount = x.Deliveries.Count,
            AssignedTeams = x.Households
                .Where(h => h.CampaignTeam != null)
                .Select(h => h.CampaignTeam!)
                .Concat(x.Deliveries.Where(d => d.CampaignTeam != null).Select(d => d.CampaignTeam!))
                .GroupBy(t => t.CampaignTeamId)
                .Select(g => new DistributionPointTeamSummaryResponse
                {
                    CampaignTeamId = g.Key,
                    CampaignTeamName = g.Select(t => t.Team?.Name).FirstOrDefault(name => !string.IsNullOrWhiteSpace(name)) ?? string.Empty
                })
                .OrderBy(t => t.CampaignTeamName)
                .ToList()
        };

        private static ReliefPackageDefinitionResponse MapPackage(ReliefPackageDefinition x) => new()
        {
            ReliefPackageDefinitionId = x.ReliefPackageDefinitionId,
            CampaignId = x.CampaignId,
            OutputSupplyItemId = x.OutputSupplyItemId,
            OutputSupplyItemName = x.OutputSupplyItem?.Name ?? string.Empty,
            OutputUnit = x.OutputSupplyItem?.Unit ?? string.Empty,
            CashSupportAmount = x.CashSupportAmount ?? 0,
            Name = x.Name,
            Description = x.Description,
            IsDefault = x.IsDefault,
            IsActive = x.IsActive,
            CreatedAt = x.CreatedAt,
            Items = x.Items.Select(i => new ReliefPackageDefinitionItemResponse
            {
                ReliefPackageDefinitionItemId = i.ReliefPackageDefinitionItemId,
                SupplyItemId = i.SupplyItemId,
                SupplyItemName = i.SupplyItem?.Name ?? string.Empty,
                Quantity = i.Quantity,
                Unit = i.Unit
            }).ToList()
        };

        private static HouseholdDeliveryResponse MapHouseholdDelivery(HouseholdDelivery x) => new()
        {
            HouseholdDeliveryId = x.HouseholdDeliveryId,
            CampaignId = x.CampaignId,
            CampaignHouseholdId = x.CampaignHouseholdId,
            DistributionPointId = x.DistributionPointId,
            DistributionPointName = x.DistributionPoint?.Name,
            CampaignTeamId = x.CampaignTeamId,
            CampaignTeamName = x.CampaignTeam?.Team?.Name,
            ReliefPackageDefinitionId = x.ReliefPackageDefinitionId,
            ReliefPackageDefinitionName = x.ReliefPackageDefinition?.Name ?? string.Empty,
            DeliveredByUserId = x.DeliveredByUserId,
            DeliveryMode = x.DeliveryMode,
            Status = x.Status,
            CashSupportAmount = x.CashSupportAmount,
            ScheduledAt = x.ScheduledAt,
            DeliveredAt = x.DeliveredAt,
            Notes = x.Notes,
            CreatedAt = x.CreatedAt,
            Proofs = x.Proofs.Select(p => new HouseholdDeliveryProofResponse
            {
                HouseholdDeliveryProofId = p.HouseholdDeliveryProofId,
                FileUrl = p.FileUrl,
                FileType = p.FileType,
                Note = p.Note,
                CapturedAt = p.CapturedAt,
                CapturedByUserId = p.CapturedByUserId
            }).ToList()
        };

        private static TeamDeliveryWorklistItemResponse MapTeamDeliveryWorklistItem(HouseholdDelivery x) => new()
        {
            HouseholdDeliveryId = x.HouseholdDeliveryId,
            CampaignHouseholdId = x.CampaignHouseholdId,
            CampaignId = x.CampaignId,
            CampaignTeamId = x.CampaignTeamId,
            CampaignTeamName = x.CampaignTeam?.Team?.Name,
            DistributionPointId = x.DistributionPointId,
            DistributionPointName = x.DistributionPoint?.Name,
            HouseholdCode = x.CampaignHousehold?.HouseholdCode ?? string.Empty,
            HeadOfHouseholdName = x.CampaignHousehold?.HeadOfHouseholdName ?? string.Empty,
            ContactPhone = x.CampaignHousehold?.ContactPhone,
            Address = x.CampaignHousehold?.Address,
            Latitude = x.CampaignHousehold?.Latitude ?? 0,
            Longitude = x.CampaignHousehold?.Longitude ?? 0,
            HouseholdSize = x.CampaignHousehold?.HouseholdSize ?? 0,
            IsIsolated = x.CampaignHousehold?.IsIsolated ?? false,
            FloodSeverityLevel = x.CampaignHousehold?.FloodSeverityLevel,
            IsolationSeverityLevel = x.CampaignHousehold?.IsolationSeverityLevel,
            RequiresBoat = x.CampaignHousehold?.RequiresBoat ?? false,
            RequiresLocalGuide = x.CampaignHousehold?.RequiresLocalGuide ?? false,
            SuggestedSupportMode = x.CampaignHousehold is null ? string.Empty : GetSuggestedSupportMode(x.CampaignHousehold),
            DeliveryMode = x.DeliveryMode,
            Status = x.Status,
            ScheduledAt = x.ScheduledAt,
            Notes = x.Notes,
            ProofCount = x.Proofs?.Count ?? 0
        };

        private static SupplyShortageRequestResponse MapShortage(SupplyShortageRequest x) => new()
        {
            SupplyShortageRequestId = x.SupplyShortageRequestId,
            CampaignId = x.CampaignId,
            DistributionPointId = x.DistributionPointId,
            DistributionPointName = x.DistributionPoint?.Name,
            CampaignTeamId = x.CampaignTeamId,
            CampaignTeamName = x.CampaignTeam?.Team?.Name,
            RequestedByUserId = x.RequestedByUserId,
            RequestedByUserName = x.RequestedByUser?.DisplayName ?? x.RequestedByUser?.UserName,
            Status = x.Status,
            Reason = x.Reason,
            RequestedAt = x.RequestedAt,
            ReviewedAt = x.ReviewedAt,
            ReviewedByUserId = x.ReviewedByUserId,
            ReviewedByUserName = x.ReviewedByUser?.DisplayName ?? x.ReviewedByUser?.UserName,
            ReviewNote = x.ReviewNote,
            Items = x.Items.Select(i => new SupplyShortageRequestItemResponse
            {
                SupplyShortageRequestItemId = i.SupplyShortageRequestItemId,
                SupplyItemId = i.SupplyItemId,
                SupplyItemName = i.SupplyItem?.Name ?? string.Empty,
                QuantityRequested = i.QuantityRequested,
                QuantityApproved = i.QuantityApproved,
                Note = i.Note
            }).ToList()
        };

        private static ReliefPackageAssemblyResponse MapPackageAssembly(ReliefPackageAssembly x) => new()
        {
            ReliefPackageAssemblyId = x.ReliefPackageAssemblyId,
            CampaignId = x.CampaignId,
            ReliefStationId = x.ReliefStationId,
            InventoryId = x.InventoryId,
            ReliefPackageDefinitionId = x.ReliefPackageDefinitionId,
            OutputSupplyItemId = x.OutputSupplyItemId,
            OutputSupplyItemName = x.OutputSupplyItem?.Name ?? string.Empty,
            OutputUnit = x.OutputSupplyItem?.Unit ?? string.Empty,
            QuantityCreated = x.QuantityCreated,
            CreatedBy = x.CreatedBy,
            CreatedAt = x.CreatedAt,
            Notes = x.Notes,
            Details = x.Details.Select(d => new ReliefPackageAssemblyConsumeItemResponse
            {
                SupplyItemId = d.SupplyItemId,
                SupplyItemName = d.SupplyItem?.Name ?? string.Empty,
                Unit = d.Unit,
                QuantityConsumed = d.QuantityConsumed
            }).ToList()
        };

        private async Task LogDistributionFailureAsync(HouseholdDelivery delivery, ReliefPackageDefinition package, decimal totalDistributionMoney, decimal reliefCampaignBalance, CancellationToken cancellationToken)
        {
            await _unitOfWork.AuditLogs.AddAsync(new AuditLog
            {
                AuditLogId = Guid.NewGuid(),
                EntityName = "HouseholdDelivery",
                Action = "DistributionFailed",
                Timestamp = DateTime.UtcNow,
                UserId = _currentUser.UserId,
                PrimaryKey = delivery.HouseholdDeliveryId.ToString(),
                NewValues = JsonSerializer.Serialize(new
                {
                    delivery.HouseholdDeliveryId,
                    delivery.CampaignId,
                    delivery.ReliefPackageDefinitionId,
                    PackageName = package.Name,
                    Quantity = 1,
                    TotalMoneyUsed = totalDistributionMoney,
                    ReliefCampaignBalance = reliefCampaignBalance,
                    Message = "Insufficient balance. Please extract more funds from fundraising campaign or create a new fundraising campaign."
                })
            });

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        private static void ValidateStationAttachedToCampaign(Campaign campaign, Guid reliefStationId)
        {
            var stationAttached = campaign.CampaignStations.Any(x => x.ReliefStationId == reliefStationId && x.IsActive);
            if (!stationAttached)
                throw new InvalidOperationException("Relief station is not attached to this campaign.");
        }
    }
}
