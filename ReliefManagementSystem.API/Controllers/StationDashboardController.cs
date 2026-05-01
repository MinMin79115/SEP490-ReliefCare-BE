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

        [HttpGet("active-dispatch")]
        public async Task<ActionResult<ActiveDispatchSnapshotResponseDto>> GetActiveDispatch(CancellationToken cancellationToken)
            => Ok(await _stationDashboardService.GetActiveDispatchSnapshotAsync(cancellationToken));
    }
}
