using Microsoft.EntityFrameworkCore;
using ReliefManagementSystem.Application.Common.Interface;
using ReliefManagementSystem.Application.Features.StationDashboard.DTOs.Response;
using ReliefManagementSystem.Application.Interface;
using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Application.Services
{
    public class StationDashboardService : IStationDashboardService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public StationDashboardService(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<StationOverviewResponseDto> GetOverviewAsync(CancellationToken cancellationToken = default)
        {
            var station = await GetCurrentModeratorStationAsync(cancellationToken);
            var stationId = station.ReliefStationId;
            var today = DateTime.UtcNow.Date;

            var allRequests = await _unitOfWork.RescueRequests.GetAllAsync(cancellationToken);
            var stationRequests = allRequests.Where(r => r.RescueOperations.Any(o => o.ReliefStationId == stationId)).ToList();

            var rescueStatus = BuildRescueStatusSummary(stationRequests, null, null);

            var activeTeams = await _unitOfWork.Teams.GetQueryable()
                .CountAsync(t => t.Status == TeamStatus.Active && t.ReliefStationTeams.Any(x => x.ReliefStationId == stationId && x.Status == ReliefTeamAssignmentStatus.Approved), cancellationToken);

            var availableVehicles = await _unitOfWork.Vehicles.GetQueryable()
                .CountAsync(v => v.ReliefStationId == stationId && v.Status == VehicleStatus.Free, cancellationToken);

            var busyVehicles = await _unitOfWork.Vehicles.GetQueryable()
                .CountAsync(v => v.ReliefStationId == stationId && v.Status == VehicleStatus.Busy, cancellationToken);

            var unreadNotifications = await _unitOfWork.Notifications.GetUnreadCountAsync(_currentUserService.UserId!.Value, cancellationToken);

            var lowStockItems = await _unitOfWork.InventoryStocks.GetQueryable()
                .CountAsync(s => s.Inventory.ReliefStationId == stationId && s.Inventory.Status != EntityStatus.Deleted && s.CurrentQuantity <= s.MinimumStockLevel && s.MinimumStockLevel > 0, cancellationToken);

            var pendingShortageRequests = await _unitOfWork.SupplyShortageRequests.GetQueryable()
                .CountAsync(s => s.CampaignTeam != null && s.CampaignTeam.Team.ReliefStationTeams.Any(rst => rst.ReliefStationId == stationId && rst.Status == ReliefTeamAssignmentStatus.Approved) && s.Status == SupplyShortageRequestStatus.Pending, cancellationToken);

            var completedToday = stationRequests.Count(r => r.RescueRequestStatus == RescueRequestStatus.Completed && r.UpdatedAt.HasValue && r.UpdatedAt.Value.Date == today);

            return new StationOverviewResponseDto
            {
                StationId = stationId,
                StationName = station.Name,
                PendingRescueRequests = rescueStatus.Pending,
                VerifiedRescueRequests = rescueStatus.Verified,
                AssignedRescueRequests = rescueStatus.Assigned,
                InProgressRescueRequests = rescueStatus.InProgress,
                CompletedToday = completedToday,
                ActiveTeams = activeTeams,
                AvailableVehicles = availableVehicles,
                BusyVehicles = busyVehicles,
                UnreadNotifications = unreadNotifications,
                LowStockItems = lowStockItems,
                PendingShortageRequests = pendingShortageRequests
            };
        }

        public async Task<RescueRequestStatusSummaryDto> GetRescueRequestStatusSummaryAsync(DateTime? from, DateTime? to, CancellationToken cancellationToken = default)
        {
            var station = await GetCurrentModeratorStationAsync(cancellationToken);
            var allRequests = await _unitOfWork.RescueRequests.GetAllAsync(cancellationToken);
            var stationRequests = allRequests.Where(r => r.RescueOperations.Any(o => o.ReliefStationId == station.ReliefStationId)).ToList();

            return BuildRescueStatusSummary(stationRequests, from, to);
        }

        public async Task<TeamPerformanceResponseDto> GetTeamPerformanceAsync(DateTime? from, DateTime? to, CancellationToken cancellationToken = default)
        {
            var station = await GetCurrentModeratorStationAsync(cancellationToken);
            var stationId = station.ReliefStationId;

            var teams = await _unitOfWork.Teams.GetQueryable()
                .Where(t => t.ReliefStationTeams.Any(x => x.ReliefStationId == stationId && x.Status == ReliefTeamAssignmentStatus.Approved))
                .ToListAsync(cancellationToken);

            var teamIds = teams.Select(t => t.TeamId).ToList();

            var rescueOperations = await _unitOfWork.RescueOperations.GetByStationIdAsync(stationId, cancellationToken);
            rescueOperations = rescueOperations
                .Where(o => o.TeamId.HasValue && teamIds.Contains(o.TeamId.Value))
                .Where(o => !from.HasValue || o.StartedAt >= from.Value)
                .Where(o => !to.HasValue || o.StartedAt <= to.Value)
                .ToList();

            var activeBatchTeamIds = (await _unitOfWork.RescueBatches.GetAllActiveWithItemsAsync(cancellationToken))
                .Where(b => teamIds.Contains(b.TeamId))
                .Select(b => b.TeamId)
                .ToList();

            var trackingByTeamId = new Dictionary<Guid, DateTime?>();
            foreach (var teamId in teamIds)
            {
                var latest = await _unitOfWork.TeamTrackingPoints.GetLatestPointAsync(teamId, cancellationToken);
                trackingByTeamId[teamId] = latest?.CapturedAtUtc;
            }

            return new TeamPerformanceResponseDto
            {
                Data = teams.Select(team =>
                {
                    var teamOps = rescueOperations.Where(o => o.TeamId == team.TeamId).ToList();
                    return new TeamPerformanceItemDto
                    {
                        TeamId = team.TeamId,
                        TeamName = team.Name,
                        TeamType = team.TeamType.ToString(),
                        AssignedRequests = teamOps.Count,
                        ActiveBatch = activeBatchTeamIds.Contains(team.TeamId),
                        InProgressRequests = teamOps.Count(o => o.Status == RescueOperationStatus.EnRoute || o.Status == RescueOperationStatus.Rescuing),
                        CompletedRequests = teamOps.Count(o => o.Status == RescueOperationStatus.RescueCompleted || o.Status == RescueOperationStatus.Closed),
                        LastTrackedAt = trackingByTeamId.GetValueOrDefault(team.TeamId)
                    };
                }).ToList()
            };
        }

        public async Task<VehicleSummaryResponseDto> GetVehicleSummaryAsync(CancellationToken cancellationToken = default)
        {
            var station = await GetCurrentModeratorStationAsync(cancellationToken);
            var vehicles = await _unitOfWork.Vehicles.GetQueryable()
                .Where(v => v.ReliefStationId == station.ReliefStationId)
                .ToListAsync(cancellationToken);

            return new VehicleSummaryResponseDto
            {
                Total = vehicles.Count,
                Available = vehicles.Count(v => v.Status == VehicleStatus.Free),
                Busy = vehicles.Count(v => v.Status == VehicleStatus.Busy),
                ByType = vehicles.GroupBy(v => v.VehicleType?.TypeName ?? "Unknown")
                    .Select(g => new VehicleTypeSummaryDto
                    {
                        VehicleTypeName = g.Key,
                        Total = g.Count(),
                        Available = g.Count(v => v.Status == VehicleStatus.Free),
                        Busy = g.Count(v => v.Status == VehicleStatus.Busy)
                    })
                    .OrderBy(x => x.VehicleTypeName)
                    .ToList()
            };
        }

        public async Task<StationAlertsSummaryDto> GetAlertsSummaryAsync(CancellationToken cancellationToken = default)
        {
            var station = await GetCurrentModeratorStationAsync(cancellationToken);
            var stationId = station.ReliefStationId;

            var unreadNotifications = await _unitOfWork.Notifications.GetUnreadCountAsync(_currentUserService.UserId!.Value, cancellationToken);

            var pendingVolunteerApplications = await _unitOfWork.VolunteerProfiles.GetQueryableForReview()
                .CountAsync(v => v.VerificationStatus == VerificationStatus.Pending, cancellationToken);

            var pendingJoinRequests = await _unitOfWork.TeamJoinRequests.GetQueryableWithDetails()
                .CountAsync(x => x.Status == TeamJoinRequestStatus.Pending && x.Team.ReliefStationTeams.Any(rst => rst.ReliefStationId == stationId && rst.Status == ReliefTeamAssignmentStatus.Approved), cancellationToken);

            var pendingShortageRequests = await _unitOfWork.SupplyShortageRequests.GetQueryable()
                .CountAsync(s => s.CampaignTeam != null && s.CampaignTeam.Team.ReliefStationTeams.Any(rst => rst.ReliefStationId == stationId && rst.Status == ReliefTeamAssignmentStatus.Approved) && s.Status == SupplyShortageRequestStatus.Pending, cancellationToken);

            var criticalStockItems = await _unitOfWork.InventoryStocks.GetQueryable()
                .CountAsync(s => s.Inventory.ReliefStationId == stationId && s.Inventory.Status != EntityStatus.Deleted && s.CurrentQuantity <= s.MinimumStockLevel && s.MinimumStockLevel > 0, cancellationToken);

            var vehiclesUnavailable = await _unitOfWork.Vehicles.GetQueryable()
                .CountAsync(v => v.ReliefStationId == stationId && v.Status == VehicleStatus.Busy, cancellationToken);

            return new StationAlertsSummaryDto
            {
                UnreadNotifications = unreadNotifications,
                PendingVolunteerApplications = pendingVolunteerApplications,
                PendingJoinRequests = pendingJoinRequests,
                PendingShortageRequests = pendingShortageRequests,
                CriticalStockItems = criticalStockItems,
                VehiclesUnavailable = vehiclesUnavailable
            };
        }

        public async Task<InventorySummaryResponseDto> GetInventorySummaryAsync(CancellationToken cancellationToken = default)
        {
            var station = await GetCurrentModeratorStationAsync(cancellationToken);
            var stationId = station.ReliefStationId;

            var stocks = await _unitOfWork.InventoryStocks.GetQueryable()
                .Where(s => s.Inventory.ReliefStationId == stationId && s.Inventory.Status != EntityStatus.Deleted)
                .ToListAsync(cancellationToken);

            return new InventorySummaryResponseDto
            {
                InventoryCount = await _unitOfWork.Inventories.GetQueryable().CountAsync(i => i.ReliefStationId == stationId, cancellationToken),
                TotalStockItems = stocks.Count,
                SafeItems = stocks.Count(s => s.CurrentQuantity > s.MinimumStockLevel),
                NeedRestockItems = stocks.Count(s => s.MinimumStockLevel > 0 && s.CurrentQuantity <= s.MinimumStockLevel && s.CurrentQuantity > 0),
                CriticalItems = stocks.Count(s => s.CurrentQuantity <= 0 || (s.MinimumStockLevel > 0 && s.CurrentQuantity <= s.MinimumStockLevel)),
                TopCriticalItems = stocks
                    .Where(s => s.MinimumStockLevel > 0)
                    .OrderBy(s => s.CurrentQuantity)
                    .Take(10)
                    .Select(s => new CriticalStockItemDto
                    {
                        SupplyItemId = s.SupplyItemId,
                        SupplyItemName = s.SupplyItem?.Name ?? string.Empty,
                        CurrentQuantity = s.CurrentQuantity,
                        MinimumStockLevel = s.MinimumStockLevel
                    })
                    .ToList()
            };
        }

        public async Task<RescueRequestTrendResponseDto> GetRescueRequestTrendAsync(DateTime? from, DateTime? to, string groupBy, CancellationToken cancellationToken = default)
        {
            var station = await GetCurrentModeratorStationAsync(cancellationToken);
            var allRequests = await _unitOfWork.RescueRequests.GetAllAsync(cancellationToken);
            var stationRequests = allRequests
                .Where(r => r.RescueOperations.Any(o => o.ReliefStationId == station.ReliefStationId))
                .Where(r => !from.HasValue || r.CreatedAt >= from.Value)
                .Where(r => !to.HasValue || r.CreatedAt <= to.Value)
                .ToList();

            var grouped = stationRequests
                .GroupBy(r => r.CreatedAt.Date)
                .OrderBy(g => g.Key)
                .Select(g => new RescueRequestTrendItemDto
                {
                    Label = g.Key.ToString("yyyy-MM-dd"),
                    Created = g.Count(),
                    Assigned = g.Count(r => r.RescueRequestStatus == RescueRequestStatus.Assigned),
                    Completed = g.Count(r => r.RescueRequestStatus == RescueRequestStatus.Completed)
                })
                .ToList();

            return new RescueRequestTrendResponseDto
            {
                GroupBy = string.IsNullOrWhiteSpace(groupBy) ? "day" : groupBy,
                Data = grouped
            };
        }

        public async Task<RescueRequestTypeSummaryResponseDto> GetRescueRequestTypeSummaryAsync(DateTime? from, DateTime? to, CancellationToken cancellationToken = default)
        {
            var station = await GetCurrentModeratorStationAsync(cancellationToken);
            var allRequests = await _unitOfWork.RescueRequests.GetAllAsync(cancellationToken);
            var stationRequests = allRequests
                .Where(r => r.RescueOperations.Any(o => o.ReliefStationId == station.ReliefStationId))
                .Where(r => !from.HasValue || r.CreatedAt >= from.Value)
                .Where(r => !to.HasValue || r.CreatedAt <= to.Value)
                .ToList();

            return new RescueRequestTypeSummaryResponseDto
            {
                Total = stationRequests.Count,
                Normal = stationRequests.Count(r => r.RescueRequestType == RescueRequestType.Normal),
                Emergency = stationRequests.Count(r => r.RescueRequestType == RescueRequestType.Emergency)
            };
        }

        public async Task<ActiveDispatchSnapshotResponseDto> GetActiveDispatchSnapshotAsync(CancellationToken cancellationToken = default)
        {
            var station = await GetCurrentModeratorStationAsync(cancellationToken);
            var operations = await _unitOfWork.RescueOperations.GetByStationIdAsync(station.ReliefStationId, cancellationToken);

            var activeOps = operations
                .Where(o => o.Status == RescueOperationStatus.EnRoute || o.Status == RescueOperationStatus.Rescuing || o.Status == RescueOperationStatus.Assigned)
                .OrderByDescending(o => o.StartedAt)
                .ToList();

            return new ActiveDispatchSnapshotResponseDto
            {
                ActiveOperations = activeOps.Select(o => new ActiveDispatchItemDto
                {
                    RequestId = o.RescueRequestId,
                    OperationId = o.RescueOperationId,
                    TeamName = o.Team?.Name ?? string.Empty,
                    Status = o.Status.ToString(),
                    Address = o.RescueRequest?.Address,
                    LastTrackedAt = o.Team?.TrackingPoints?.OrderByDescending(x => x.CapturedAtUtc).FirstOrDefault()?.CapturedAtUtc,
                    Vehicles = o.RescueOperationVehicles.Select(v => new SimpleAssignedVehicleDto
                    {
                        VehicleId = v.VehicleId,
                        VehicleName = v.Vehicle?.VehicleType?.TypeName,
                        VehicleLicensePlate = v.Vehicle?.LicensePlate,
                        IsPrimary = v.IsPrimary
                    }).ToList()
                }).ToList()
            };
        }

        public async Task<RescueRequestLocationsResponseDto> GetRescueRequestLocationsAsync(DateTime? from, DateTime? to, CancellationToken cancellationToken = default)
        {
            var station = await GetCurrentModeratorStationAsync(cancellationToken);
            var allRequests = await _unitOfWork.RescueRequests.GetAllAsync(cancellationToken);

            var items = allRequests
                .Where(r => r.RescueOperations.Any(o => o.ReliefStationId == station.ReliefStationId))
                .Where(r => !from.HasValue || r.CreatedAt >= from.Value)
                .Where(r => !to.HasValue || r.CreatedAt <= to.Value)
                .Where(r => r.Latitude != 0 && r.Longitude != 0)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new RescueRequestLocationItemDto
                {
                    RequestId = r.RequestId,
                    Address = r.Address,
                    Latitude = r.Latitude,
                    Longitude = r.Longitude,
                    RescueRequestType = r.RescueRequestType.ToString(),
                    RescueRequestStatus = r.RescueRequestStatus.ToString(),
                    CreatedAt = r.CreatedAt
                })
                .ToList();

            return new RescueRequestLocationsResponseDto
            {
                Items = items
            };
        }

        public async Task<ReliefTeamMissionSnapshotResponseDto> GetReliefTeamMissionSnapshotAsync(
            DateTime? from,
            DateTime? to,
            IEnumerable<Guid>? teamIds,
            CancellationToken cancellationToken = default)
        {
            var station = await GetCurrentModeratorStationAsync(cancellationToken);
            var stationId = station.ReliefStationId;
            var requestedTeamIds = teamIds?
                .Where(id => id != Guid.Empty)
                .Distinct()
                .ToHashSet();

            var campaignIds = await _unitOfWork.Campaigns.GetQueryable()
                .AsNoTracking()
                .Where(c => c.Type == CampaignType.Relief)
                .Where(c =>
                    c.CampaignStations.Any(cs => cs.ReliefStationId == stationId && cs.IsActive) ||
                    c.CampaignTeams.Any(ct =>
                        !ct.IsDelete &&
                        ct.Team.ReliefStationTeams.Any(rst =>
                            rst.ReliefStationId == stationId &&
                            rst.Status == ReliefTeamAssignmentStatus.Approved)))
                .Select(c => c.CampaignId)
                .Distinct()
                .ToListAsync(cancellationToken);

            if (campaignIds.Count == 0)
            {
                return new ReliefTeamMissionSnapshotResponseDto();
            }

            var campaignTaskRows = await _unitOfWork.CampaignTasks.GetQueryable()
                .AsNoTracking()
                .Where(ct =>
                    !ct.CampaignTeam.IsDelete &&
                    campaignIds.Contains(ct.CampaignTeam.CampaignId) &&
                    (
                        ct.CampaignTeam.Campaign.CampaignStations.Any(cs =>
                            cs.ReliefStationId == stationId &&
                            cs.IsActive) ||
                        ct.CampaignTeam.Team.ReliefStationTeams.Any(rst =>
                            rst.ReliefStationId == stationId &&
                            rst.Status == ReliefTeamAssignmentStatus.Approved)
                    ) &&
                    (requestedTeamIds == null || requestedTeamIds.Contains(ct.CampaignTeam.TeamId)))
                .Select(ct => new
                {
                    ct.CampaignTaskId,
                    ct.CampaignTeamId,
                    CampaignId = ct.CampaignTeam.CampaignId,
                    CampaignName = ct.CampaignTeam.Campaign.Name,
                    CampaignStatus = ct.CampaignTeam.Campaign.Status,
                    CampaignTeamStatus = ct.CampaignTeam.Status,
                    TeamId = ct.CampaignTeam.TeamId,
                    TeamName = ct.CampaignTeam.Team.Name,
                    TeamType = ct.CampaignTeam.Team.TeamType,
                    CampaignTaskStatus = ct.Status,
                    TaskCreatedAt = ct.CreatedAt,
                    TaskStartDate = ct.StartDate,
                    ct.DueDate,
                })
                .ToListAsync(cancellationToken);

            if (from.HasValue)
            {
                campaignTaskRows = campaignTaskRows.Where(ct =>
                    ct.TaskCreatedAt >= from.Value ||
                    ct.TaskStartDate >= from.Value ||
                    (ct.DueDate.HasValue && ct.DueDate.Value >= from.Value))
                    .ToList();
            }

            if (to.HasValue)
            {
                campaignTaskRows = campaignTaskRows.Where(ct =>
                    ct.TaskCreatedAt <= to.Value ||
                    ct.TaskStartDate <= to.Value ||
                    (ct.DueDate.HasValue && ct.DueDate.Value <= to.Value))
                    .ToList();
            }

            if (campaignTaskRows.Count == 0)
            {
                return new ReliefTeamMissionSnapshotResponseDto();
            }

            var campaignTaskIds = campaignTaskRows.Select(x => x.CampaignTaskId).Distinct().ToList();

            var memberTaskRows = await _unitOfWork.MemberTasks.GetQueryable()
                .AsNoTracking()
                .Where(mt => campaignTaskIds.Contains(mt.CampaignTaskId))
                .Select(mt => new
                {
                    CampaignTaskId = mt.CampaignTaskId,
                    CampaignTeamId = mt.CampaignTask.CampaignTeamId,
                    CampaignId = mt.CampaignTask.CampaignTeam.CampaignId,
                    CampaignName = mt.CampaignTask.CampaignTeam.Campaign.Name,
                    CampaignStatus = mt.CampaignTask.CampaignTeam.Campaign.Status,
                    CampaignTeamStatus = mt.CampaignTask.CampaignTeam.Status,
                    TeamId = mt.CampaignTask.CampaignTeam.TeamId,
                    TeamName = mt.CampaignTask.CampaignTeam.Team.Name,
                    TeamType = mt.CampaignTask.CampaignTeam.Team.TeamType,
                    CampaignTaskStatus = mt.CampaignTask.Status,
                    TaskCreatedAt = mt.CampaignTask.CreatedAt,
                    TaskStartDate = mt.CampaignTask.StartDate,
                    MemberTaskStatus = mt.Status,
                    AssignedAt = mt.AssignedAt,
                    CompletedAt = mt.CompletedAt,
                })
                .ToListAsync(cancellationToken);

            var taskStatsByTaskId = memberTaskRows
                .GroupBy(x => x.CampaignTaskId)
                .ToDictionary(
                    group => group.Key,
                    group => new
                    {
                        TotalSubTasks = group.Count(),
                        AssignedSubTasks = group.Count(mt => mt.MemberTaskStatus == MemberTaskStatus.Assigned),
                        InProgressSubTasks = group.Count(mt => mt.MemberTaskStatus == MemberTaskStatus.InProgress),
                        CompletedSubTasks = group.Count(mt => mt.MemberTaskStatus == MemberTaskStatus.Completed),
                        FailedSubTasks = group.Count(mt => mt.MemberTaskStatus == MemberTaskStatus.Failed),
                        CancelledSubTasks = group.Count(mt => mt.MemberTaskStatus == MemberTaskStatus.Cancelled),
                        LastTaskUpdatedAt = group
                            .Select(t => t.CompletedAt ?? t.AssignedAt ?? t.TaskStartDate)
                            .OrderByDescending(x => x)
                            .FirstOrDefault()
                    });

            var campaignTeamIds = campaignTaskRows.Select(x => x.CampaignTeamId).Distinct().ToList();
            var filteredCampaignIds = campaignTaskRows.Select(x => x.CampaignId).Distinct().ToList();

            var packageDefinitions = await _unitOfWork.ReliefPackageDefinitions.GetQueryable()
                .AsNoTracking()
                .Where(x => filteredCampaignIds.Contains(x.CampaignId))
                .ToListAsync(cancellationToken);

            var deliveries = await _unitOfWork.HouseholdDeliveries.GetQueryable()
                .AsNoTracking()
                .Where(x => x.CampaignTeamId.HasValue && campaignTeamIds.Contains(x.CampaignTeamId.Value))
                .ToListAsync(cancellationToken);

            var data = campaignTaskRows
                .GroupBy(t => new
                {
                    t.TeamId,
                    t.CampaignTeamId,
                    t.CampaignId,
                    t.TeamName,
                    t.TeamType,
                    t.CampaignName,
                    t.CampaignStatus,
                    t.CampaignTeamStatus,
                })
                .Select(group => new ReliefTeamMissionSnapshotItemDto
                    {
                        TeamId = group.Key.TeamId,
                        CampaignTeamId = group.Key.CampaignTeamId,
                        CampaignId = group.Key.CampaignId,
                        TeamName = group.Key.TeamName,
                        TeamType = group.Key.TeamType.ToString(),
                        CampaignName = group.Key.CampaignName,
                        CampaignStatus = group.Key.CampaignStatus.ToString(),
                        CampaignTeamStatus = group.Key.CampaignTeamStatus.ToString(),
                        TotalTasks = group.Select(t => t.CampaignTaskId).Distinct().Count(),
                        PlannedTasks = group.Where(t => t.CampaignTaskStatus == CampaignTaskStatus.Planned).Select(t => t.CampaignTaskId).Distinct().Count(),
                        InProgressTasks = group.Where(t => t.CampaignTaskStatus == CampaignTaskStatus.InProgress).Select(t => t.CampaignTaskId).Distinct().Count(),
                        BlockedTasks = group.Where(t => t.CampaignTaskStatus == CampaignTaskStatus.Blocked).Select(t => t.CampaignTaskId).Distinct().Count(),
                        CompletedTasks = group.Where(t => t.CampaignTaskStatus == CampaignTaskStatus.Completed).Select(t => t.CampaignTaskId).Distinct().Count(),
                        CancelledTasks = group.Where(t => t.CampaignTaskStatus == CampaignTaskStatus.Cancelled).Select(t => t.CampaignTaskId).Distinct().Count(),
                        TotalSubTasks = group.Sum(t => taskStatsByTaskId.TryGetValue(t.CampaignTaskId, out var stats) ? stats.TotalSubTasks : 0),
                        AssignedSubTasks = group.Sum(t => taskStatsByTaskId.TryGetValue(t.CampaignTaskId, out var stats) ? stats.AssignedSubTasks : 0),
                        InProgressSubTasks = group.Sum(t => taskStatsByTaskId.TryGetValue(t.CampaignTaskId, out var stats) ? stats.InProgressSubTasks : 0),
                        CompletedSubTasks = group.Sum(t => taskStatsByTaskId.TryGetValue(t.CampaignTaskId, out var stats) ? stats.CompletedSubTasks : 0),
                        FailedSubTasks = group.Sum(t => taskStatsByTaskId.TryGetValue(t.CampaignTaskId, out var stats) ? stats.FailedSubTasks : 0),
                        CancelledSubTasks = group.Sum(t => taskStatsByTaskId.TryGetValue(t.CampaignTaskId, out var stats) ? stats.CancelledSubTasks : 0),
                        HouseholdCount = deliveries
                            .Where(d => d.CampaignTeamId == group.Key.CampaignTeamId)
                            .Select(d => d.CampaignHouseholdId)
                            .Distinct()
                            .Count(),
                        PendingHouseholdCount = deliveries
                            .Where(d => d.CampaignTeamId == group.Key.CampaignTeamId)
                            .GroupBy(d => d.CampaignHouseholdId)
                            .Select(deliveryGroup => deliveryGroup
                                .OrderByDescending(x => x.DeliveredAt ?? x.ScheduledAt)
                                .ThenByDescending(x => x.CreatedAt)
                                .First())
                            .Count(d => d.Status != HouseholdFulfillmentStatus.Delivered),
                        DeliveredHouseholdCount = deliveries
                            .Where(d => d.CampaignTeamId == group.Key.CampaignTeamId)
                            .GroupBy(d => d.CampaignHouseholdId)
                            .Select(deliveryGroup => deliveryGroup
                                .OrderByDescending(x => x.DeliveredAt ?? x.ScheduledAt)
                                .ThenByDescending(x => x.CreatedAt)
                                .First())
                            .Count(d => d.Status == HouseholdFulfillmentStatus.Delivered),
                        TotalDeliveryCount = deliveries.Count(d => d.CampaignTeamId == group.Key.CampaignTeamId),
                        PendingDeliveryCount = deliveries.Count(d => d.CampaignTeamId == group.Key.CampaignTeamId && d.Status != HouseholdFulfillmentStatus.Delivered),
                        DeliveredDeliveryCount = deliveries.Count(d => d.CampaignTeamId == group.Key.CampaignTeamId && d.Status == HouseholdFulfillmentStatus.Delivered),
                        DefaultReliefPackageName = packageDefinitions
                            .Where(p => p.CampaignId == group.Key.CampaignId)
                            .OrderByDescending(p => p.IsDefault)
                            .ThenBy(p => p.Name)
                            .Select(p => p.Name)
                            .FirstOrDefault(),
                        LastTaskUpdatedAt = group
                            .Select(t => taskStatsByTaskId.TryGetValue(t.CampaignTaskId, out var stats) ? stats.LastTaskUpdatedAt : (t.TaskStartDate as DateTime?))
                            .OrderByDescending(x => x)
                            .FirstOrDefault()
                    })
                .OrderByDescending(x => x.InProgressTasks)
                .ThenByDescending(x => x.CompletedSubTasks)
                .ThenBy(x => x.TeamName)
                .ToList();

            return new ReliefTeamMissionSnapshotResponseDto
            {
                Data = data
            };
        }

        public async Task<ReliefTeamTaskSummaryResponseDto> GetReliefTeamTaskSummaryAsync(
            DateTime? from,
            DateTime? to,
            IEnumerable<Guid>? teamIds,
            CancellationToken cancellationToken = default)
        {
            var station = await GetCurrentModeratorStationAsync(cancellationToken);
            var stationId = station.ReliefStationId;
            var requestedTeamIds = teamIds?
                .Where(id => id != Guid.Empty)
                .Distinct()
                .ToHashSet();

            var campaignIds = await _unitOfWork.Campaigns.GetQueryable()
                .AsNoTracking()
                .Where(c => c.Type == CampaignType.Relief)
                .Where(c =>
                    c.CampaignStations.Any(cs => cs.ReliefStationId == stationId && cs.IsActive) ||
                    c.CampaignTeams.Any(ct =>
                        !ct.IsDelete &&
                        ct.Team.ReliefStationTeams.Any(rst =>
                            rst.ReliefStationId == stationId &&
                            rst.Status == ReliefTeamAssignmentStatus.Approved)))
                .Select(c => c.CampaignId)
                .Distinct()
                .ToListAsync(cancellationToken);

            if (campaignIds.Count == 0)
            {
                return new ReliefTeamTaskSummaryResponseDto();
            }

            var campaignTaskRows = await _unitOfWork.CampaignTasks.GetQueryable()
                .AsNoTracking()
                .Where(ct =>
                    !ct.CampaignTeam.IsDelete &&
                    campaignIds.Contains(ct.CampaignTeam.CampaignId) &&
                    (
                        ct.CampaignTeam.Campaign.CampaignStations.Any(cs =>
                            cs.ReliefStationId == stationId &&
                            cs.IsActive) ||
                        ct.CampaignTeam.Team.ReliefStationTeams.Any(rst =>
                            rst.ReliefStationId == stationId &&
                            rst.Status == ReliefTeamAssignmentStatus.Approved)
                    ) &&
                    (requestedTeamIds == null || requestedTeamIds.Contains(ct.CampaignTeam.TeamId)))
                .Where(ct =>
                    !from.HasValue ||
                    ct.CreatedAt >= from.Value ||
                    ct.StartDate >= from.Value ||
                    (ct.DueDate.HasValue && ct.DueDate.Value >= from.Value))
                .Where(ct =>
                    !to.HasValue ||
                    ct.CreatedAt <= to.Value ||
                    ct.StartDate <= to.Value ||
                    (ct.DueDate.HasValue && ct.DueDate.Value <= to.Value))
                .Select(ct => new
                {
                    ct.CampaignTaskId,
                    ct.CampaignTeamId,
                    CampaignId = ct.CampaignTeam.CampaignId,
                    CampaignName = ct.CampaignTeam.Campaign.Name,
                    CampaignStatus = ct.CampaignTeam.Campaign.Status,
                    CampaignTeamStatus = ct.CampaignTeam.Status,
                    TeamId = ct.CampaignTeam.TeamId,
                    TeamName = ct.CampaignTeam.Team.Name,
                    TeamType = ct.CampaignTeam.Team.TeamType,
                    Title = ct.Title,
                    CampaignTaskStatus = ct.Status,
                    ct.StartDate,
                    ct.DueDate,
                })
                .ToListAsync(cancellationToken);

            if (campaignTaskRows.Count == 0)
            {
                return new ReliefTeamTaskSummaryResponseDto();
            }

            var campaignTaskIds = campaignTaskRows.Select(x => x.CampaignTaskId).Distinct().ToList();
            var campaignTeamIds = campaignTaskRows.Select(x => x.CampaignTeamId).Distinct().ToList();
            var filteredCampaignIds = campaignTaskRows.Select(x => x.CampaignId).Distinct().ToList();

            var memberTaskRows = await _unitOfWork.MemberTasks.GetQueryable()
                .AsNoTracking()
                .Where(mt => campaignTaskIds.Contains(mt.CampaignTaskId))
                .Select(mt => new
                {
                    mt.CampaignTaskId,
                    MemberTaskStatus = mt.Status,
                    LastUpdatedAt = mt.CompletedAt ?? mt.AssignedAt,
                    DeliveryCount = mt.MemberTaskDeliveries.Count(),
                    PendingDeliveryCount = mt.MemberTaskDeliveries.Count(mtd => mtd.HouseholdDelivery.Status != HouseholdFulfillmentStatus.Delivered),
                    DeliveredDeliveryCount = mt.MemberTaskDeliveries.Count(mtd => mtd.HouseholdDelivery.Status == HouseholdFulfillmentStatus.Delivered),
                })
                .ToListAsync(cancellationToken);

            var memberTaskStatsByTaskId = memberTaskRows
                .GroupBy(x => x.CampaignTaskId)
                .ToDictionary(
                    group => group.Key,
                    group => new
                    {
                        TotalSubTasks = group.Count(),
                        AssignedSubTasks = group.Count(x => x.MemberTaskStatus == MemberTaskStatus.Assigned),
                        InProgressSubTasks = group.Count(x => x.MemberTaskStatus == MemberTaskStatus.InProgress),
                        CompletedSubTasks = group.Count(x => x.MemberTaskStatus == MemberTaskStatus.Completed),
                        FailedSubTasks = group.Count(x => x.MemberTaskStatus == MemberTaskStatus.Failed),
                        CancelledSubTasks = group.Count(x => x.MemberTaskStatus == MemberTaskStatus.Cancelled),
                        DeliveryCount = group.Sum(x => x.DeliveryCount),
                        PendingDeliveryCount = group.Sum(x => x.PendingDeliveryCount),
                        DeliveredDeliveryCount = group.Sum(x => x.DeliveredDeliveryCount),
                        LastUpdatedAt = group
                            .Select(x => x.LastUpdatedAt)
                            .Where(x => x.HasValue)
                            .OrderByDescending(x => x)
                            .FirstOrDefault(),
                    });

            var packageDefinitions = await _unitOfWork.ReliefPackageDefinitions.GetQueryable()
                .AsNoTracking()
                .Where(x => filteredCampaignIds.Contains(x.CampaignId))
                .ToListAsync(cancellationToken);

            var deliveries = await _unitOfWork.HouseholdDeliveries.GetQueryable()
                .AsNoTracking()
                .Where(x => x.CampaignTeamId.HasValue && campaignTeamIds.Contains(x.CampaignTeamId.Value))
                .ToListAsync(cancellationToken);

            var data = campaignTaskRows.GroupBy(t => new
            {
                t.TeamId,
                t.TeamName,
                t.TeamType,
                t.CampaignId,
                t.CampaignName,
                t.CampaignStatus,
                t.CampaignTeamId,
                t.CampaignTeamStatus,
            }).Select(group =>
            {
                var teamTasks = group
                    .OrderBy(t => t.StartDate)
                    .ThenBy(t => t.Title)
                    .Select(t =>
                    {
                        memberTaskStatsByTaskId.TryGetValue(t.CampaignTaskId, out var stats);
                        return new ReliefTeamTaskSummaryTaskDto
                        {
                            CampaignTaskId = t.CampaignTaskId,
                            Title = t.Title,
                            Status = t.CampaignTaskStatus.ToString(),
                            StartDate = t.StartDate,
                            DueDate = t.DueDate,
                            TotalSubTasks = stats?.TotalSubTasks ?? 0,
                            AssignedSubTasks = stats?.AssignedSubTasks ?? 0,
                            InProgressSubTasks = stats?.InProgressSubTasks ?? 0,
                            CompletedSubTasks = stats?.CompletedSubTasks ?? 0,
                            FailedSubTasks = stats?.FailedSubTasks ?? 0,
                            CancelledSubTasks = stats?.CancelledSubTasks ?? 0,
                            DeliveryCount = stats?.DeliveryCount ?? 0,
                            PendingDeliveryCount = stats?.PendingDeliveryCount ?? 0,
                            DeliveredDeliveryCount = stats?.DeliveredDeliveryCount ?? 0,
                            LastUpdatedAt = stats?.LastUpdatedAt,
                        };
                    })
                    .ToList();

                var latestHouseholds = deliveries
                    .Where(d => d.CampaignTeamId == group.Key.CampaignTeamId)
                    .GroupBy(d => d.CampaignHouseholdId)
                    .Select(group => group.OrderByDescending(x => x.DeliveredAt ?? x.ScheduledAt).ThenByDescending(x => x.CreatedAt).First())
                    .ToList();

                return new ReliefTeamTaskSummaryItemDto
                {
                    TeamId = group.Key.TeamId,
                    TeamName = group.Key.TeamName,
                    TeamType = group.Key.TeamType.ToString(),
                    CampaignId = group.Key.CampaignId,
                    CampaignName = group.Key.CampaignName,
                    CampaignStatus = group.Key.CampaignStatus.ToString(),
                    CampaignTeamId = group.Key.CampaignTeamId,
                    CampaignTeamStatus = group.Key.CampaignTeamStatus.ToString(),
                    HouseholdCount = latestHouseholds.Count,
                    PendingHouseholdCount = latestHouseholds.Count(x => x.Status != HouseholdFulfillmentStatus.Delivered),
                    DeliveredHouseholdCount = latestHouseholds.Count(x => x.Status == HouseholdFulfillmentStatus.Delivered),
                    TotalDeliveryCount = deliveries.Count(d => d.CampaignTeamId == group.Key.CampaignTeamId),
                    DefaultReliefPackageName = packageDefinitions
                        .Where(p => p.CampaignId == group.Key.CampaignId)
                        .OrderByDescending(p => p.IsDefault)
                        .ThenBy(p => p.Name)
                        .Select(p => p.Name)
                        .FirstOrDefault(),
                    Tasks = teamTasks,
                };
            })
            .OrderByDescending(x => x.PendingHouseholdCount)
            .ThenBy(x => x.TeamName)
            .ToList();

            return new ReliefTeamTaskSummaryResponseDto
            {
                Data = data
            };
        }

        private async Task<ReliefStation> GetCurrentModeratorStationAsync(CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId ?? throw new UnauthorizedAccessException("User not authenticated.");
            var profile = await _unitOfWork.ModeratorProfiles.GetByUserIdAsync(userId, cancellationToken)
                ?? throw new InvalidOperationException("Moderator profile not found.");

            return profile.ReliefStation ?? throw new InvalidOperationException("Moderator is not assigned to a relief station.");
        }

        private static RescueRequestStatusSummaryDto BuildRescueStatusSummary(
            IEnumerable<RescueRequest> requests,
            DateTime? from,
            DateTime? to)
        {
            if (from.HasValue)
            {
                requests = requests.Where(r => r.CreatedAt >= from.Value);
            }

            if (to.HasValue)
            {
                requests = requests.Where(r => r.CreatedAt <= to.Value);
            }

            var requestList = requests.ToList();

            return new RescueRequestStatusSummaryDto
            {
                Total = requestList.Count,
                Pending = requestList.Count(r => r.RescueRequestStatus == RescueRequestStatus.Pending),
                Verified = requestList.Count(r => r.RescueRequestStatus == RescueRequestStatus.Verified),
                Assigned = requestList.Count(r => r.RescueRequestStatus == RescueRequestStatus.Assigned),
                InProgress = requestList.Count(r => r.RescueRequestStatus == RescueRequestStatus.InProgress),
                Completed = requestList.Count(r => r.RescueRequestStatus == RescueRequestStatus.Completed),
                Cancelled = requestList.Count(r => r.RescueRequestStatus == RescueRequestStatus.Cancelled)
            };
        }
    }
}
