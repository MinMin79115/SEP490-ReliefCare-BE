using Microsoft.EntityFrameworkCore;
using ReliefManagementSystem.Application.Common.Interface;
using ReliefManagementSystem.Application.Common.Models;
using ReliefManagementSystem.Application.Features.StationReports.DTOs.Response;
using ReliefManagementSystem.Application.Interface;
using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Application.Services
{
    public class StationReportService : IStationReportService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public StationReportService(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<Pagination<RescueRequestReportItemDto>> GetRescueRequestsReportAsync(DateTime? from, DateTime? to, string? status, int pageIndex, int pageSize, CancellationToken cancellationToken = default)
        {
            var stationId = await GetCurrentStationIdAsync(cancellationToken);
            var requests = await _unitOfWork.RescueRequests.GetAllAsync(cancellationToken);

            var items = requests
                .Where(r => r.RescueOperations.Any(o => o.ReliefStationId == stationId))
                .Where(r => !from.HasValue || r.CreatedAt >= from.Value)
                .Where(r => !to.HasValue || r.CreatedAt <= to.Value)
                .Where(r => string.IsNullOrWhiteSpace(status) || r.RescueRequestStatus.ToString().Equals(status, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(r => r.CreatedAt)
                .Select(r =>
                {
                    var latestOp = r.RescueOperations.OrderByDescending(o => o.StartedAt).FirstOrDefault();
                    return new RescueRequestReportItemDto
                    {
                        RequestId = r.RequestId,
                        Address = r.Address,
                        RescueRequestType = r.RescueRequestType.ToString(),
                        Status = r.RescueRequestStatus.ToString(),
                        TeamName = latestOp?.Team?.Name,
                        PrimaryVehicle = latestOp?.Vehicle != null ? $"{latestOp.Vehicle.VehicleType?.TypeName} - {latestOp.Vehicle.LicensePlate}" : null,
                        CreatedAt = r.CreatedAt
                    };
                })
                .ToList();

            return ToPagination(items, pageIndex, pageSize);
        }

        public async Task<List<TeamWorkloadReportItemDto>> GetTeamWorkloadReportAsync(DateTime? from, DateTime? to, CancellationToken cancellationToken = default)
        {
            var stationId = await GetCurrentStationIdAsync(cancellationToken);
            var teams = await _unitOfWork.Teams.GetQueryable()
                .Where(t => t.ReliefStationTeams.Any(x => x.ReliefStationId == stationId && x.Status == ReliefTeamAssignmentStatus.Approved))
                .ToListAsync(cancellationToken);
            var ops = await _unitOfWork.RescueOperations.GetByStationIdAsync(stationId, cancellationToken);
            var activeBatches = await _unitOfWork.RescueBatches.GetAllActiveWithItemsAsync(cancellationToken);

            return teams.Select(t =>
            {
                var teamOps = ops.Where(o => o.TeamId == t.TeamId)
                    .Where(o => !from.HasValue || o.StartedAt >= from.Value)
                    .Where(o => !to.HasValue || o.StartedAt <= to.Value)
                    .ToList();
                var memberCount = _unitOfWork.TeamMembers.GetQueryable().Count(tm => tm.TeamId == t.TeamId);
                return new TeamWorkloadReportItemDto
                {
                    TeamId = t.TeamId,
                    TeamName = t.Name,
                    AssignedRequests = teamOps.Count,
                    CompletedRequests = teamOps.Count(o => o.Status == RescueOperationStatus.RescueCompleted || o.Status == RescueOperationStatus.Closed),
                    ActiveBatchCount = activeBatches.Count(b => b.TeamId == t.TeamId),
                    MemberCount = memberCount
                };
            }).ToList();
        }

        public async Task<List<VehicleUtilizationReportItemDto>> GetVehicleUtilizationReportAsync(DateTime? from, DateTime? to, CancellationToken cancellationToken = default)
        {
            var stationId = await GetCurrentStationIdAsync(cancellationToken);
            var vehicles = await _unitOfWork.Vehicles.GetQueryable().Where(v => v.ReliefStationId == stationId).ToListAsync(cancellationToken);
            var ops = await _unitOfWork.RescueOperations.GetByStationIdAsync(stationId, cancellationToken);

            return vehicles.Select(v =>
            {
                var relatedOps = ops.Where(o => o.RescueOperationVehicles.Any(rov => rov.VehicleId == v.VehicleId))
                    .Where(o => !from.HasValue || o.StartedAt >= from.Value)
                    .Where(o => !to.HasValue || o.StartedAt <= to.Value)
                    .ToList();
                return new VehicleUtilizationReportItemDto
                {
                    VehicleId = v.VehicleId,
                    VehicleName = v.VehicleType?.TypeName,
                    VehicleLicensePlate = v.LicensePlate,
                    BusyCount = relatedOps.Count(o => o.Status == RescueOperationStatus.EnRoute || o.Status == RescueOperationStatus.Rescuing || o.Status == RescueOperationStatus.Assigned),
                    UsedInOperations = relatedOps.Count,
                    IsCurrentlyBusy = v.Status == VehicleStatus.Busy
                };
            }).ToList();
        }

        public async Task<Pagination<InventoryStockReportItemDto>> GetInventoryStockReportAsync(Guid? inventoryId, string? status, int pageIndex, int pageSize, CancellationToken cancellationToken = default)
        {
            var stationId = await GetCurrentStationIdAsync(cancellationToken);
            var query = _unitOfWork.InventoryStocks.GetQueryable()
                .Where(s => s.Inventory.ReliefStationId == stationId && s.Inventory.Status != EntityStatus.Deleted);

            if (inventoryId.HasValue)
            {
                query = query.Where(s => s.InventoryId == inventoryId.Value);
            }

            var items = await query.ToListAsync(cancellationToken);
            var mapped = items.Select(s => new InventoryStockReportItemDto
            {
                InventoryStockId = s.InventoryStockId,
                SupplyItemName = s.SupplyItem?.Name ?? string.Empty,
                CurrentQuantity = s.CurrentQuantity,
                MinimumStockLevel = s.MinimumStockLevel,
                MaximumStockLevel = s.MaximumStockLevel,
                InventoryStatus = s.InventoryStatus.ToString()
            });

            if (!string.IsNullOrWhiteSpace(status))
            {
                mapped = mapped.Where(x => x.InventoryStatus.Equals(status, StringComparison.OrdinalIgnoreCase));
            }

            return ToPagination(mapped.OrderBy(x => x.SupplyItemName).ToList(), pageIndex, pageSize);
        }

        public async Task<Pagination<ReliefDeliveryReportItemDto>> GetReliefDeliveriesReportAsync(Guid? campaignId, string? status, int pageIndex, int pageSize, CancellationToken cancellationToken = default)
        {
            var stationId = await GetCurrentStationIdAsync(cancellationToken);
            var query = _unitOfWork.HouseholdDeliveries.GetQueryable()
                .Where(x => x.CampaignTeam != null && x.CampaignTeam.Team.ReliefStationTeams.Any(rst => rst.ReliefStationId == stationId && rst.Status == ReliefTeamAssignmentStatus.Approved));

            if (campaignId.HasValue)
            {
                query = query.Where(x => x.CampaignId == campaignId.Value);
            }

            if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<HouseholdFulfillmentStatus>(status, true, out var parsedStatus))
            {
                query = query.Where(x => x.Status == parsedStatus);
            }

            var items = await query.OrderByDescending(x => x.ScheduledAt).ToListAsync(cancellationToken);

            var mapped = items.Select(x => new ReliefDeliveryReportItemDto
            {
                HouseholdCode = x.CampaignHousehold?.HouseholdCode ?? string.Empty,
                HeadOfHouseholdName = x.CampaignHousehold?.HeadOfHouseholdName ?? string.Empty,
                Address = x.CampaignHousehold?.Address,
                TeamName = x.CampaignTeam?.Team?.Name,
                DeliveryMode = x.DeliveryMode.ToString(),
                FulfillmentStatus = x.Status.ToString()
            }).ToList();

            return ToPagination(mapped, pageIndex, pageSize);
        }

        private async Task<Guid> GetCurrentStationIdAsync(CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId ?? throw new UnauthorizedAccessException("User not authenticated.");
            var profile = await _unitOfWork.ModeratorProfiles.GetByUserIdAsync(userId, cancellationToken)
                ?? throw new InvalidOperationException("Moderator profile not found.");
            return profile.ReliefStationId ?? throw new InvalidOperationException("Moderator is not assigned to a relief station.");
        }

        private static Pagination<T> ToPagination<T>(List<T> items, int pageIndex, int pageSize)
        {
            pageIndex = pageIndex <= 0 ? 1 : pageIndex;
            pageSize = pageSize <= 0 ? 10 : pageSize;
            var total = items.Count;
            var paged = items.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
            return new Pagination<T>(paged, total, pageIndex, pageSize);
        }
    }
}
