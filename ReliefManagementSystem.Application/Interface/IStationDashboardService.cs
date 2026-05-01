using ReliefManagementSystem.Application.Features.StationDashboard.DTOs.Response;

namespace ReliefManagementSystem.Application.Interface
{
    public interface IStationDashboardService
    {
        Task<StationOverviewResponseDto> GetOverviewAsync(CancellationToken cancellationToken = default);
        Task<RescueRequestStatusSummaryDto> GetRescueRequestStatusSummaryAsync(DateTime? from, DateTime? to, CancellationToken cancellationToken = default);
        Task<TeamPerformanceResponseDto> GetTeamPerformanceAsync(DateTime? from, DateTime? to, CancellationToken cancellationToken = default);
        Task<VehicleSummaryResponseDto> GetVehicleSummaryAsync(CancellationToken cancellationToken = default);
        Task<StationAlertsSummaryDto> GetAlertsSummaryAsync(CancellationToken cancellationToken = default);
        Task<InventorySummaryResponseDto> GetInventorySummaryAsync(CancellationToken cancellationToken = default);
        Task<RescueRequestTrendResponseDto> GetRescueRequestTrendAsync(DateTime? from, DateTime? to, string groupBy, CancellationToken cancellationToken = default);
        Task<ActiveDispatchSnapshotResponseDto> GetActiveDispatchSnapshotAsync(CancellationToken cancellationToken = default);
    }
}
