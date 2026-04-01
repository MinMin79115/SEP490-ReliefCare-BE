using ReliefManagementSystem.Application.Common.Interface;
using ReliefManagementSystem.Application.Features.DistributionSession.DTOs.Request;
using ReliefManagementSystem.Application.Features.DistributionSession.DTOs.Response;
using ReliefManagementSystem.Application.Interface;
using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Application.Services
{
    public class DistributionSessionService : IDistributionSessionService
    {
        private readonly IUnitOfWork _unitOfWork;

        public DistributionSessionService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<DistributionSessionResponseDto> CreateAsync(CreateDistributionSessionRequest request, CancellationToken cancellationToken = default)
        {
            await ValidateCampaignStationAsync(request.CampaignId, request.ReliefStationId, cancellationToken);
            ValidateLocationRules(request);

            var entity = new Domain.Entities.DistributionSession
            {
                DistributionSessionId = Guid.NewGuid(),
                CampaignId = request.CampaignId,
                ReliefStationId = request.ReliefStationId,
                Mode = request.Mode,
                Name = request.Name,
                ScheduledStartAt = request.ScheduledStartAt,
                ScheduledEndAt = request.ScheduledEndAt,
                LocationName = request.LocationName,
                Address = request.Address,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                RadiusMeters = request.RadiusMeters,
                Notes = request.Notes,
                Status = DistributionSessionStatus.Draft,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.DistributionSessions.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return await GetByIdAsync(entity.DistributionSessionId, cancellationToken);
        }

        public async Task<DistributionSessionResponseDto> GetByIdAsync(Guid distributionSessionId, CancellationToken cancellationToken = default)
        {
            var session = await _unitOfWork.DistributionSessions.GetByIdAsync(distributionSessionId, cancellationToken);
            if (session == null)
                throw new KeyNotFoundException($"Distribution session '{distributionSessionId}' was not found.");

            return MapToResponse(session);
        }

        public async Task<PaginatedDistributionSessionResponseDto> SearchAsync(SearchDistributionSessionRequest request, CancellationToken cancellationToken = default)
        {
            DistributionSessionStatus? status = null;
            if (request.StatusFilter.HasValue)
            {
                status = (DistributionSessionStatus)request.StatusFilter.Value;
            }

            var (items, totalCount) = await _unitOfWork.DistributionSessions.SearchAsync(
                request.Search,
                request.PageNumber,
                request.PageSize,
                status,
                request.CampaignId,
                request.ReliefStationId,
                cancellationToken);

            return new PaginatedDistributionSessionResponseDto
            {
                Data = items.Select(MapToResponse).ToList(),
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }

        public async Task<DistributionSessionResponseDto> AddItemsAsync(Guid distributionSessionId, AddDistributionSessionItemsRequest request, CancellationToken cancellationToken = default)
        {
            var session = await _unitOfWork.DistributionSessions.GetByIdAsync(distributionSessionId, cancellationToken);
            if (session == null)
                throw new KeyNotFoundException($"Distribution session '{distributionSessionId}' was not found.");

            EnsureStatus(session.Status, DistributionSessionStatus.Draft, DistributionSessionStatus.Ready);

            foreach (var item in request.Items)
            {
                if (!await _unitOfWork.SupplyItems.ExistsAsync(item.SupplyItemId))
                    throw new KeyNotFoundException($"Supply item '{item.SupplyItemId}' was not found.");

                SupplyAllocationItem? allocationItem = null;
                if (item.SupplyAllocationItemId.HasValue)
                {
                    allocationItem = await FindAllocationItemAsync(item.SupplyAllocationItemId.Value, session.CampaignId, cancellationToken);
                    if (allocationItem == null)
                        throw new InvalidOperationException("SupplyAllocationItem does not belong to an allocation of the same campaign.");
                }

                var existing = session.Items.FirstOrDefault(x =>
                    x.SupplyItemId == item.SupplyItemId &&
                    x.SupplyAllocationItemId == item.SupplyAllocationItemId);

                if (existing != null)
                {
                    existing.ReservedQuantity += item.ReservedQuantity;
                }
                else
                {
                    session.Items.Add(new DistributionSessionItem
                    {
                        DistributionSessionItemId = Guid.NewGuid(),
                        DistributionSessionId = session.DistributionSessionId,
                        SupplyItemId = item.SupplyItemId,
                        SupplyAllocationItemId = item.SupplyAllocationItemId,
                        ReservedQuantity = item.ReservedQuantity,
                        DeliveredQuantity = 0
                    });
                }
            }

            session.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return await GetByIdAsync(distributionSessionId, cancellationToken);
        }

        public async Task<DistributionSessionResponseDto> AttachRequestsAsync(Guid distributionSessionId, AttachRequestsToSessionRequest request, CancellationToken cancellationToken = default)
        {
            var session = await _unitOfWork.DistributionSessions.GetByIdAsync(distributionSessionId, cancellationToken);
            if (session == null)
                throw new KeyNotFoundException($"Distribution session '{distributionSessionId}' was not found.");

            EnsureStatus(session.Status, DistributionSessionStatus.Draft, DistributionSessionStatus.Ready);

            foreach (var requestInput in request.Requests)
            {
                var reliefRequest = await _unitOfWork.ReliefRequests.GetByIdAsync(requestInput.ReliefRequestId, cancellationToken);
                if (reliefRequest == null)
                    throw new KeyNotFoundException($"Relief request '{requestInput.ReliefRequestId}' was not found.");

                if (reliefRequest.AssignedReliefStationId != session.ReliefStationId)
                    throw new InvalidOperationException("Relief request is assigned to a different station.");

                if (reliefRequest.CampaignId.HasValue && reliefRequest.CampaignId != session.CampaignId)
                    throw new InvalidOperationException("Relief request belongs to a different campaign.");

                if (reliefRequest.Status != ReliefRequestStatus.Approved && reliefRequest.Status != ReliefRequestStatus.Allocated)
                    throw new InvalidOperationException("Only Approved or Allocated relief requests can be attached to a distribution session.");

                if (await _unitOfWork.DistributionSessions.ExistsRequestAssignmentAsync(session.DistributionSessionId, reliefRequest.RequestId, cancellationToken))
                    continue;

                session.Requests.Add(new DistributionSessionRequest
                {
                    DistributionSessionId = session.DistributionSessionId,
                    ReliefRequestId = reliefRequest.RequestId,
                    PlannedNote = requestInput.PlannedNote
                });

                if (reliefRequest.Status == ReliefRequestStatus.Approved)
                {
                    reliefRequest.Status = ReliefRequestStatus.Allocated;
                    reliefRequest.UpdatedAt = DateTime.UtcNow;
                }
            }

            session.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return await GetByIdAsync(distributionSessionId, cancellationToken);
        }

        public async Task<DistributionSessionResponseDto> MarkReadyAsync(Guid distributionSessionId, CancellationToken cancellationToken = default)
        {
            var session = await _unitOfWork.DistributionSessions.GetByIdAsync(distributionSessionId, cancellationToken);
            if (session == null)
                throw new KeyNotFoundException($"Distribution session '{distributionSessionId}' was not found.");

            if (session.Status != DistributionSessionStatus.Draft)
                throw new InvalidOperationException("Only draft session can be marked ready.");

            if (!session.Items.Any())
                throw new InvalidOperationException("Distribution session must have at least one item before marking ready.");

            if (!session.Requests.Any())
                throw new InvalidOperationException("Distribution session must have at least one attached request before marking ready.");

            session.Status = DistributionSessionStatus.Ready;
            session.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return await GetByIdAsync(distributionSessionId, cancellationToken);
        }

        public async Task<DistributionSessionResponseDto> StartAsync(Guid distributionSessionId, CancellationToken cancellationToken = default)
        {
            var session = await _unitOfWork.DistributionSessions.GetByIdAsync(distributionSessionId, cancellationToken);
            if (session == null)
                throw new KeyNotFoundException($"Distribution session '{distributionSessionId}' was not found.");

            if (session.Status != DistributionSessionStatus.Ready)
                throw new InvalidOperationException("Only ready session can be started.");

            session.Status = DistributionSessionStatus.InProgress;
            session.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return await GetByIdAsync(distributionSessionId, cancellationToken);
        }

        public async Task<DistributionSessionResponseDto> CompleteAsync(Guid distributionSessionId, CancellationToken cancellationToken = default)
        {
            var session = await _unitOfWork.DistributionSessions.GetByIdAsync(distributionSessionId, cancellationToken);
            if (session == null)
                throw new KeyNotFoundException($"Distribution session '{distributionSessionId}' was not found.");

            if (session.Status != DistributionSessionStatus.InProgress)
                throw new InvalidOperationException("Only in-progress session can be completed.");

            session.Status = DistributionSessionStatus.Completed;
            session.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return await GetByIdAsync(distributionSessionId, cancellationToken);
        }

        public async Task<DistributionSessionResponseDto> CancelAsync(Guid distributionSessionId, CancellationToken cancellationToken = default)
        {
            var session = await _unitOfWork.DistributionSessions.GetByIdAsync(distributionSessionId, cancellationToken);
            if (session == null)
                throw new KeyNotFoundException($"Distribution session '{distributionSessionId}' was not found.");

            EnsureStatus(session.Status, DistributionSessionStatus.Draft, DistributionSessionStatus.Ready);

            session.Status = DistributionSessionStatus.Cancelled;
            session.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return await GetByIdAsync(distributionSessionId, cancellationToken);
        }

        private async Task ValidateCampaignStationAsync(Guid campaignId, Guid reliefStationId, CancellationToken cancellationToken)
        {
            var campaign = await _unitOfWork.Campaigns.GetWithStationsAsync(campaignId, cancellationToken);
            if (campaign == null)
                throw new KeyNotFoundException($"Campaign '{campaignId}' was not found.");

            if (campaign.Type != CampaignType.Relief)
                throw new InvalidOperationException("Distribution session only supports relief campaign.");

            var activeStation = campaign.CampaignStations.FirstOrDefault(cs => cs.IsActive);
            if (activeStation == null || activeStation.ReliefStationId != reliefStationId)
                throw new InvalidOperationException("Campaign does not have the specified active station attached.");
        }

        private static void ValidateLocationRules(CreateDistributionSessionRequest request)
        {
            if (request.Mode == DistributionSessionMode.Centralized
                && string.IsNullOrWhiteSpace(request.Address)
                && string.IsNullOrWhiteSpace(request.LocationName))
            {
                throw new InvalidOperationException("Centralized distribution session requires address or location name.");
            }
        }

        private async Task<SupplyAllocationItem?> FindAllocationItemAsync(Guid allocationItemId, Guid campaignId, CancellationToken cancellationToken)
        {
            var allocations = await _unitOfWork.SupplyAllocations.GetByCampaignIdAsync(campaignId, cancellationToken);
            return allocations.SelectMany(a => a.Items).FirstOrDefault(i => i.AllocationItemId == allocationItemId);
        }

        private static void EnsureStatus(DistributionSessionStatus current, params DistributionSessionStatus[] allowed)
        {
            if (!allowed.Contains(current))
            {
                throw new InvalidOperationException($"Current distribution session status '{current}' is not allowed for this action.");
            }
        }

        private static DistributionSessionResponseDto MapToResponse(Domain.Entities.DistributionSession session)
        {
            return new DistributionSessionResponseDto
            {
                DistributionSessionId = session.DistributionSessionId,
                CampaignId = session.CampaignId,
                CampaignName = session.Campaign?.Name ?? string.Empty,
                ReliefStationId = session.ReliefStationId,
                ReliefStationName = session.ReliefStation?.Name ?? string.Empty,
                Name = session.Name,
                Mode = session.Mode,
                Status = session.Status,
                ScheduledStartAt = session.ScheduledStartAt,
                ScheduledEndAt = session.ScheduledEndAt,
                LocationName = session.LocationName,
                Address = session.Address,
                Latitude = session.Latitude,
                Longitude = session.Longitude,
                RadiusMeters = session.RadiusMeters,
                Notes = session.Notes,
                CreatedAt = session.CreatedAt,
                UpdatedAt = session.UpdatedAt,
                Items = session.Items.Select(i => new DistributionSessionItemResponseDto
                {
                    DistributionSessionItemId = i.DistributionSessionItemId,
                    SupplyItemId = i.SupplyItemId,
                    SupplyItemName = i.SupplyItem?.Name ?? string.Empty,
                    SupplyAllocationItemId = i.SupplyAllocationItemId,
                    ReservedQuantity = i.ReservedQuantity,
                    DeliveredQuantity = i.DeliveredQuantity
                }).ToList(),
                Requests = session.Requests.Select(r => new DistributionSessionRequestResponseDto
                {
                    ReliefRequestId = r.ReliefRequestId,
                    ReporterFullName = r.ReliefRequest?.ReporterFullName ?? string.Empty,
                    ReporterPhone = r.ReliefRequest?.ReporterPhone ?? string.Empty,
                    Address = r.ReliefRequest?.Address,
                    ReliefRequestStatus = r.ReliefRequest?.Status.ToString() ?? string.Empty,
                    PlannedNote = r.PlannedNote
                }).ToList()
            };
        }
    }
}
