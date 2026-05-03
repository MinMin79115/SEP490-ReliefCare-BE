using ReliefManagementSystem.Application.Common.Models;
using ReliefManagementSystem.Application.Features.StationReports.DTOs.Response;

namespace ReliefManagementSystem.Application.Interface
{
    public interface IStationReportService
    {
        Task<Pagination<RescueRequestReportItemDto>> GetRescueRequestsReportAsync(DateTime? from, DateTime? to, string? status, int pageIndex, int pageSize, CancellationToken cancellationToken = default);
        Task<List<TeamWorkloadReportItemDto>> GetTeamWorkloadReportAsync(DateTime? from, DateTime? to, CancellationToken cancellationToken = default);
        Task<List<VehicleUtilizationReportItemDto>> GetVehicleUtilizationReportAsync(DateTime? from, DateTime? to, CancellationToken cancellationToken = default);
        Task<Pagination<InventoryStockReportItemDto>> GetInventoryStockReportAsync(Guid? inventoryId, string? status, int pageIndex, int pageSize, CancellationToken cancellationToken = default);
        Task<Pagination<ReliefDeliveryReportItemDto>> GetReliefDeliveriesReportAsync(Guid? campaignId, string? status, int pageIndex, int pageSize, CancellationToken cancellationToken = default);
        Task<List<ReliefMissionReportRowDto>> GetReliefMissionReportAsync(DateTime? from, DateTime? to, IEnumerable<Guid>? teamIds, CancellationToken cancellationToken = default);
        Task<List<ReliefMissionCampaignSummaryDto>> GetReliefMissionCampaignSummaryAsync(DateTime? from, DateTime? to, IEnumerable<Guid>? teamIds, CancellationToken cancellationToken = default);
    }
}
