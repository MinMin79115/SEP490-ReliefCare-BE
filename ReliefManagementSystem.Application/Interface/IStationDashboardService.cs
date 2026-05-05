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
        Task<RescueRequestTypeSummaryResponseDto> GetRescueRequestTypeSummaryAsync(DateTime? from, DateTime? to, CancellationToken cancellationToken = default);
        Task<ActiveDispatchSnapshotResponseDto> GetActiveDispatchSnapshotAsync(CancellationToken cancellationToken = default);
        Task<RescueRequestLocationsResponseDto> GetRescueRequestLocationsAsync(DateTime? from, DateTime? to, CancellationToken cancellationToken = default);
        Task<ReliefTeamMissionSnapshotResponseDto> GetReliefTeamMissionSnapshotAsync(DateTime? from, DateTime? to, IEnumerable<Guid>? teamIds, CancellationToken cancellationToken = default);
        Task<ReliefTeamTaskSummaryResponseDto> GetReliefTeamTaskSummaryAsync(DateTime? from, DateTime? to, IEnumerable<Guid>? teamIds, CancellationToken cancellationToken = default);
        Task<AdminTopResponseTeamsResponseDto> GetAdminTopResponseTeamsAsync(DateTime? from, DateTime? to, int top = 4, CancellationToken cancellationToken = default);
    }
}
