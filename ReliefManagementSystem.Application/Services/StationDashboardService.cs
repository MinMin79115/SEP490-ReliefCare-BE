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
