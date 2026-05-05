using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReliefManagementSystem.Application.Features.StationDashboard.DTOs.Response;
using ReliefManagementSystem.Application.Interface;

namespace ReliefManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/station-dashboard")]
    [Authorize(Roles = "Moderator")]
    public class StationDashboardController : ControllerBase
    {
        private readonly IStationDashboardService _stationDashboardService;

        public StationDashboardController(IStationDashboardService stationDashboardService)
        {
            _stationDashboardService = stationDashboardService;
        }

        [HttpGet("overview")]
        public async Task<ActionResult<StationOverviewResponseDto>> GetOverview(CancellationToken cancellationToken)
            => Ok(await _stationDashboardService.GetOverviewAsync(cancellationToken));

        [HttpGet("rescue-request-status")]
        public async Task<ActionResult<RescueRequestStatusSummaryDto>> GetRescueRequestStatus([FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken cancellationToken)
            => Ok(await _stationDashboardService.GetRescueRequestStatusSummaryAsync(from, to, cancellationToken));

        [HttpGet("team-performance")]
        public async Task<ActionResult<TeamPerformanceResponseDto>> GetTeamPerformance([FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken cancellationToken)
            => Ok(await _stationDashboardService.GetTeamPerformanceAsync(from, to, cancellationToken));

        [HttpGet("vehicle-summary")]
        public async Task<ActionResult<VehicleSummaryResponseDto>> GetVehicleSummary(CancellationToken cancellationToken)
            => Ok(await _stationDashboardService.GetVehicleSummaryAsync(cancellationToken));

        [HttpGet("alerts")]
        public async Task<ActionResult<StationAlertsSummaryDto>> GetAlerts(CancellationToken cancellationToken)
            => Ok(await _stationDashboardService.GetAlertsSummaryAsync(cancellationToken));

        [HttpGet("inventory-summary")]
        public async Task<ActionResult<InventorySummaryResponseDto>> GetInventorySummary(CancellationToken cancellationToken)
            => Ok(await _stationDashboardService.GetInventorySummaryAsync(cancellationToken));

        [HttpGet("rescue-request-trend")]
        public async Task<ActionResult<RescueRequestTrendResponseDto>> GetRescueRequestTrend([FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] string groupBy = "day", CancellationToken cancellationToken = default)
            => Ok(await _stationDashboardService.GetRescueRequestTrendAsync(from, to, groupBy, cancellationToken));

        [HttpGet("rescue-request-type-summary")]
        public async Task<ActionResult<RescueRequestTypeSummaryResponseDto>> GetRescueRequestTypeSummary([FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken cancellationToken = default)
            => Ok(await _stationDashboardService.GetRescueRequestTypeSummaryAsync(from, to, cancellationToken));

        [HttpGet("active-dispatch")]
        public async Task<ActionResult<ActiveDispatchSnapshotResponseDto>> GetActiveDispatch(CancellationToken cancellationToken)
            => Ok(await _stationDashboardService.GetActiveDispatchSnapshotAsync(cancellationToken));

        [HttpGet("rescue-request-locations")]
        public async Task<ActionResult<RescueRequestLocationsResponseDto>> GetRescueRequestLocations([FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken cancellationToken)
            => Ok(await _stationDashboardService.GetRescueRequestLocationsAsync(from, to, cancellationToken));

        [HttpGet("relief-team-missions")]
        public async Task<ActionResult<ReliefTeamMissionSnapshotResponseDto>> GetReliefTeamMissions(
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            [FromQuery] List<Guid>? teamIds,
            CancellationToken cancellationToken)
            => Ok(await _stationDashboardService.GetReliefTeamMissionSnapshotAsync(from, to, teamIds, cancellationToken));

        [HttpGet("relief-team-task-summary")]
        public async Task<ActionResult<ReliefTeamTaskSummaryResponseDto>> GetReliefTeamTaskSummary(
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            [FromQuery] List<Guid>? teamIds,
            CancellationToken cancellationToken)
            => Ok(await _stationDashboardService.GetReliefTeamTaskSummaryAsync(from, to, teamIds, cancellationToken));

        [HttpGet("admin/top-response-teams")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<AdminTopResponseTeamsResponseDto>> GetAdminTopResponseTeams(
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            [FromQuery] int top = 4,
            CancellationToken cancellationToken = default)
            => Ok(await _stationDashboardService.GetAdminTopResponseTeamsAsync(from, to, top, cancellationToken));
    }
}
