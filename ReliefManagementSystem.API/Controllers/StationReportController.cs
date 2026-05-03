using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReliefManagementSystem.Application.Common.Models;
using ReliefManagementSystem.Application.Features.StationReports.DTOs.Response;
using ReliefManagementSystem.Application.Interface;

namespace ReliefManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/station-reports")]
    [Authorize(Roles = "Moderator")]
    public class StationReportController : ControllerBase
    {
        private readonly IStationReportService _stationReportService;

        public StationReportController(IStationReportService stationReportService)
        {
            _stationReportService = stationReportService;
        }

        [HttpGet("rescue-requests")]
        public async Task<ActionResult<Pagination<RescueRequestReportItemDto>>> GetRescueRequests([FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] string? status, [FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
            => Ok(await _stationReportService.GetRescueRequestsReportAsync(from, to, status, pageIndex, pageSize, cancellationToken));

        [HttpGet("team-workload")]
        public async Task<ActionResult<List<TeamWorkloadReportItemDto>>> GetTeamWorkload([FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken cancellationToken = default)
            => Ok(await _stationReportService.GetTeamWorkloadReportAsync(from, to, cancellationToken));

        [HttpGet("vehicle-utilization")]
        public async Task<ActionResult<List<VehicleUtilizationReportItemDto>>> GetVehicleUtilization([FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken cancellationToken = default)
            => Ok(await _stationReportService.GetVehicleUtilizationReportAsync(from, to, cancellationToken));

        [HttpGet("inventory-stock")]
        public async Task<ActionResult<Pagination<InventoryStockReportItemDto>>> GetInventoryStock([FromQuery] Guid? inventoryId, [FromQuery] string? status, [FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 50, CancellationToken cancellationToken = default)
            => Ok(await _stationReportService.GetInventoryStockReportAsync(inventoryId, status, pageIndex, pageSize, cancellationToken));

        [HttpGet("relief-deliveries")]
        public async Task<ActionResult<Pagination<ReliefDeliveryReportItemDto>>> GetReliefDeliveries([FromQuery] Guid? campaignId, [FromQuery] string? status, [FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 50, CancellationToken cancellationToken = default)
            => Ok(await _stationReportService.GetReliefDeliveriesReportAsync(campaignId, status, pageIndex, pageSize, cancellationToken));

        [HttpGet("relief-missions")]
        public async Task<ActionResult<List<ReliefMissionReportRowDto>>> GetReliefMissions(
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            [FromQuery] List<Guid>? teamIds,
            CancellationToken cancellationToken = default)
            => Ok(await _stationReportService.GetReliefMissionReportAsync(from, to, teamIds, cancellationToken));

        [HttpGet("relief-missions/campaign-summary")]
        public async Task<ActionResult<List<ReliefMissionCampaignSummaryDto>>> GetReliefMissionCampaignSummary(
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            [FromQuery] List<Guid>? teamIds,
            CancellationToken cancellationToken = default)
            => Ok(await _stationReportService.GetReliefMissionCampaignSummaryAsync(from, to, teamIds, cancellationToken));
    }
}
