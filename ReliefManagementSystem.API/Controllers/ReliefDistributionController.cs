using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReliefManagementSystem.Application.Common.Models;
using ReliefManagementSystem.Application.Features.Relief.DTOs.Request;
using ReliefManagementSystem.Application.Features.Relief.DTOs.Response;
using ReliefManagementSystem.Application.Interface;
using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.API.Controllers
{
    [Route("api/relief/campaigns/{campaignId:guid}")]
    [ApiController]
    [Authorize]
    public class ReliefDistributionController : ControllerBase
    {
        private readonly IReliefDistributionService _reliefDistributionService;

        public ReliefDistributionController(IReliefDistributionService reliefDistributionService)
        {
            _reliefDistributionService = reliefDistributionService;
        }

        [HttpPost("households/import")]
        [Authorize(Roles = "Manager,Moderator")]
        public async Task<IActionResult> ImportHouseholds(
            Guid campaignId,
            [FromBody] ImportCampaignHouseholdsRequest request,
            CancellationToken cancellationToken)
            => Ok(await _reliefDistributionService.ImportCampaignHouseholdsAsync(campaignId, request, cancellationToken));

        [HttpPost("households/report-new")]
        [Authorize(Roles = "Manager,Moderator,Volunteer")]
        public async Task<IActionResult> ReportNewReliefHousehold(
            Guid campaignId,
            [FromBody] ReportNewReliefHouseholdRequest request,
            CancellationToken cancellationToken)
            => Ok(await _reliefDistributionService.ReportNewReliefHouseholdAsync(campaignId, request, cancellationToken));

        [HttpPatch("households/{campaignHouseholdId:guid}/assign")]
        [Authorize(Roles = "Manager,Moderator")]
        public async Task<IActionResult> AssignHousehold(
            Guid campaignId,
            Guid campaignHouseholdId,
            [FromBody] AssignHouseholdRequest request,
            CancellationToken cancellationToken)
            => Ok(await _reliefDistributionService.AssignHouseholdAsync(campaignId, campaignHouseholdId, request, cancellationToken));

        [HttpPatch("households/{campaignHouseholdId:guid}/assign-isolated-team")]
        [Authorize(Roles = "Manager,Moderator")]
        public async Task<IActionResult> AssignIsolatedHouseholdTeam(
            Guid campaignId,
            Guid campaignHouseholdId,
            [FromBody] AssignIsolatedHouseholdTeamRequest request,
            CancellationToken cancellationToken)
            => Ok(await _reliefDistributionService.AssignIsolatedHouseholdTeamAsync(campaignId, campaignHouseholdId, request, cancellationToken));

        [HttpPatch("households/isolated-team/bulk-assign")]
        [Authorize(Roles = "Manager,Moderator")]
        public async Task<IActionResult> BulkAssignIsolatedHouseholdTeams(
            Guid campaignId,
            [FromBody] BulkAssignIsolatedHouseholdsRequest request,
            CancellationToken cancellationToken)
            => Ok(await _reliefDistributionService.BulkAssignIsolatedHouseholdTeamsAsync(campaignId, request, cancellationToken));

        [HttpGet("households")]
        [Authorize(Roles = "Manager,Moderator,Volunteer")]
        public async Task<ActionResult<Pagination<CampaignHouseholdResponse>>> GetHouseholds(
            Guid campaignId,
            [FromQuery] HouseholdQueryRequest request,
            CancellationToken cancellationToken)
            => Ok(await _reliefDistributionService.GetCampaignHouseholdsAsync(campaignId, request, cancellationToken));

        [HttpGet("plan-summary")]
        [Authorize(Roles = "Manager,Moderator,Volunteer")]
        public async Task<ActionResult<ReliefCampaignPlanSummaryResponse>> GetPlanSummary(
            Guid campaignId,
            CancellationToken cancellationToken)
            => Ok(await _reliefDistributionService.GetCampaignPlanSummaryAsync(campaignId, cancellationToken));

        [HttpPatch("households/{campaignHouseholdId:guid}")]
        [Authorize(Roles = "Manager,Moderator")]
        public async Task<IActionResult> UpdateHousehold(
            Guid campaignId,
            Guid campaignHouseholdId,
            [FromBody] UpdateCampaignHouseholdRequest request,
            CancellationToken cancellationToken)
            => Ok(await _reliefDistributionService.UpdateCampaignHouseholdAsync(campaignId, campaignHouseholdId, request, cancellationToken));

        [HttpPatch("households/{campaignHouseholdId:guid}/status")]
        [Authorize(Roles = "Manager,Moderator,Volunteer")]
        public async Task<IActionResult> UpdateHouseholdStatus(
            Guid campaignId,
            Guid campaignHouseholdId,
            [FromBody] UpdateCampaignHouseholdStatusRequest request,
            CancellationToken cancellationToken)
            => Ok(await _reliefDistributionService.UpdateCampaignHouseholdStatusAsync(campaignId, campaignHouseholdId, request, cancellationToken));

        [HttpDelete("households/{campaignHouseholdId:guid}")]
        [Authorize(Roles = "Manager,Moderator")]
        public async Task<IActionResult> DeleteHousehold(
            Guid campaignId,
            Guid campaignHouseholdId,
            CancellationToken cancellationToken)
        {
            await _reliefDistributionService.DeleteCampaignHouseholdAsync(campaignId, campaignHouseholdId, cancellationToken);
            return NoContent();
        }

        [HttpGet("checklist")]
        [Authorize(Roles = "Manager,Moderator,Volunteer")]
        public async Task<ActionResult<Pagination<HouseholdChecklistItemResponse>>> GetChecklist(
            Guid campaignId,
            [FromQuery] DeliveryQueryRequest request,
            CancellationToken cancellationToken)
            => Ok(await _reliefDistributionService.GetChecklistAsync(campaignId, request, cancellationToken));

        [HttpGet("team-worklist")]
        [Authorize(Roles = "Manager,Moderator,Volunteer")]
        public async Task<ActionResult<Pagination<TeamDeliveryWorklistItemResponse>>> GetTeamWorklist(
            Guid campaignId,
            [FromQuery] TeamDeliveryWorklistQueryRequest request,
            CancellationToken cancellationToken)
            => Ok(await _reliefDistributionService.GetTeamDeliveryWorklistAsync(campaignId, request, cancellationToken));

        [HttpPost("distribution-points")]
        [Authorize(Roles = "Manager,Moderator")]
        public async Task<IActionResult> CreateDistributionPoint(
            Guid campaignId,
            [FromBody] CreateDistributionPointRequest request,
            CancellationToken cancellationToken)
            => Ok(await _reliefDistributionService.CreateDistributionPointAsync(campaignId, request, cancellationToken));

        [HttpGet("distribution-points")]
        [Authorize(Roles = "Manager,Moderator,Volunteer")]
        public async Task<ActionResult<Pagination<DistributionPointResponse>>> GetDistributionPoints(Guid campaignId, [FromQuery] DistributionPointQueryRequest request, CancellationToken cancellationToken)
            => Ok(await _reliefDistributionService.GetDistributionPointsAsync(campaignId, request, cancellationToken));

        [HttpPatch("distribution-points/{distributionPointId:guid}")]
        [Authorize(Roles = "Manager,Moderator")]
        public async Task<IActionResult> UpdateDistributionPoint(
            Guid campaignId,
            Guid distributionPointId,
            [FromBody] UpdateDistributionPointRequest request,
            CancellationToken cancellationToken)
            => Ok(await _reliefDistributionService.UpdateDistributionPointAsync(campaignId, distributionPointId, request, cancellationToken));

        [HttpDelete("distribution-points/{distributionPointId:guid}")]
        [Authorize(Roles = "Manager,Moderator")]
        public async Task<IActionResult> DeleteDistributionPoint(Guid campaignId, Guid distributionPointId, CancellationToken cancellationToken)
            => Ok(await _reliefDistributionService.DeleteDistributionPointAsync(campaignId, distributionPointId, cancellationToken));

        [HttpPost("packages")]
        [Authorize(Roles = "Manager,Moderator")]
        public async Task<IActionResult> CreateReliefPackage(
            Guid campaignId,
            [FromBody] CreateReliefPackageDefinitionRequest request,
            CancellationToken cancellationToken)
            => Ok(await _reliefDistributionService.CreateReliefPackageDefinitionAsync(campaignId, request, cancellationToken));

        [HttpGet("packages")]
        [Authorize(Roles = "Manager,Moderator,Volunteer")]
        public async Task<ActionResult<Pagination<ReliefPackageDefinitionResponse>>> GetReliefPackages(Guid campaignId, [FromQuery] ReliefPackageQueryRequest request, CancellationToken cancellationToken)
            => Ok(await _reliefDistributionService.GetReliefPackageDefinitionsAsync(campaignId, request, cancellationToken));

        [HttpPatch("packages/{reliefPackageDefinitionId:guid}")]
        [Authorize(Roles = "Manager,Moderator")]
        public async Task<IActionResult> UpdateReliefPackage(
            Guid campaignId,
            Guid reliefPackageDefinitionId,
            [FromBody] UpdateReliefPackageDefinitionRequest request,
            CancellationToken cancellationToken)
            => Ok(await _reliefDistributionService.UpdateReliefPackageDefinitionAsync(campaignId, reliefPackageDefinitionId, request, cancellationToken));

        [HttpDelete("packages/{reliefPackageDefinitionId:guid}")]
        [Authorize(Roles = "Manager,Moderator")]
        public async Task<IActionResult> DeleteReliefPackage(Guid campaignId, Guid reliefPackageDefinitionId, CancellationToken cancellationToken)
            => Ok(await _reliefDistributionService.DeleteReliefPackageDefinitionAsync(campaignId, reliefPackageDefinitionId, cancellationToken));

        [HttpGet("packages/{reliefPackageDefinitionId:guid}/assembly-availability")]
        [Authorize(Roles = "Manager,Moderator,Volunteer")]
        public async Task<IActionResult> GetPackageAssemblyAvailability(
            Guid campaignId,
            Guid reliefPackageDefinitionId,
            [FromQuery] Guid reliefStationId,
            [FromQuery] Guid inventoryId,
            CancellationToken cancellationToken)
            => Ok(await _reliefDistributionService.GetPackageAssemblyAvailabilityAsync(
                campaignId,
                reliefPackageDefinitionId,
                reliefStationId,
                inventoryId,
                cancellationToken));

        [HttpPost("packages/{reliefPackageDefinitionId:guid}/assemble")]
        [Authorize(Roles = "Manager,Moderator")]
        public async Task<IActionResult> AssembleReliefPackage(
            Guid campaignId,
            Guid reliefPackageDefinitionId,
            [FromBody] AssembleReliefPackageRequest request,
            CancellationToken cancellationToken)
            => Ok(await _reliefDistributionService.AssembleReliefPackageAsync(
                campaignId,
                reliefPackageDefinitionId,
                request,
                cancellationToken));

        [HttpGet("package-assemblies")]
        [Authorize(Roles = "Manager,Moderator,Volunteer")]
        public async Task<IActionResult> GetPackageAssemblyHistoryByCampaign(
            Guid campaignId,
            CancellationToken cancellationToken)
            => Ok(await _reliefDistributionService.GetPackageAssemblyHistoryByCampaignAsync(campaignId, cancellationToken));

        [HttpGet("stations/{reliefStationId:guid}/package-assemblies")]
        [Authorize(Roles = "Manager,Moderator,Volunteer")]
        public async Task<IActionResult> GetPackageAssemblyHistoryByStation(
            Guid campaignId,
            Guid reliefStationId,
            CancellationToken cancellationToken)
            => Ok(await _reliefDistributionService.GetPackageAssemblyHistoryByStationAsync(campaignId, reliefStationId, cancellationToken));

        [HttpGet("packages/{reliefPackageDefinitionId:guid}/package-assemblies")]
        [Authorize(Roles = "Manager,Moderator,Volunteer")]
        public async Task<IActionResult> GetPackageAssemblyHistoryByDefinition(
            Guid campaignId,
            Guid reliefPackageDefinitionId,
            CancellationToken cancellationToken)
            => Ok(await _reliefDistributionService.GetPackageAssemblyHistoryByDefinitionAsync(campaignId, reliefPackageDefinitionId, cancellationToken));

        [HttpPost("deliveries/{householdDeliveryId:guid}/complete")]
        [Authorize(Roles = "Manager,Moderator,Volunteer")]
        public async Task<IActionResult> CompleteDelivery(
            Guid campaignId,
            Guid householdDeliveryId,
            [FromBody] CompleteHouseholdDeliveryRequest request,
            CancellationToken cancellationToken)
            => Ok(await _reliefDistributionService.CompleteHouseholdDeliveryAsync(campaignId, householdDeliveryId, request, cancellationToken));

        [HttpPost("deliveries/complete-batch")]
        [Authorize(Roles = "Manager,Moderator,Volunteer")]
        public async Task<IActionResult> CompleteDeliveryBatch(
            Guid campaignId,
            [FromBody] CompleteHouseholdDeliveryBatchRequest request,
            CancellationToken cancellationToken)
            => Ok(await _reliefDistributionService.CompleteHouseholdDeliveriesBatchAsync(campaignId, request, cancellationToken));

        [HttpGet("deliveries")]
        [Authorize(Roles = "Manager,Moderator,Volunteer")]
        public async Task<ActionResult<Pagination<HouseholdDeliveryResponse>>> GetDeliveries(
            Guid campaignId,
            [FromQuery] DeliveryQueryRequest request,
            CancellationToken cancellationToken)
            => Ok(await _reliefDistributionService.GetDeliveriesAsync(campaignId, request, cancellationToken));

        [HttpGet("deliveries/{householdDeliveryId:guid}")]
        [Authorize(Roles = "Manager,Moderator,Volunteer")]
        public async Task<IActionResult> GetDeliveryById(Guid campaignId, Guid householdDeliveryId, CancellationToken cancellationToken)
            => Ok(await _reliefDistributionService.GetDeliveryByIdAsync(campaignId, householdDeliveryId, cancellationToken));

        [HttpPatch("deliveries/{householdDeliveryId:guid}")]
        [Authorize(Roles = "Manager,Moderator")]
        public async Task<IActionResult> UpdateDeliveryAssignment(
            Guid campaignId,
            Guid householdDeliveryId,
            [FromBody] UpdateHouseholdDeliveryAssignmentRequest request,
            CancellationToken cancellationToken)
            => Ok(await _reliefDistributionService.UpdateHouseholdDeliveryAssignmentAsync(campaignId, householdDeliveryId, request, cancellationToken));

        [HttpDelete("deliveries/{householdDeliveryId:guid}")]
        [Authorize(Roles = "Manager,Moderator")]
        public async Task<IActionResult> DeleteDeliveryAssignment(
            Guid campaignId,
            Guid householdDeliveryId,
            CancellationToken cancellationToken)
        {
            await _reliefDistributionService.DeleteHouseholdDeliveryAssignmentAsync(campaignId, householdDeliveryId, cancellationToken);
            return NoContent();
        }

        [HttpPost("shortage-requests")]
        [Authorize(Roles = "Manager,Moderator,Volunteer")]
        public async Task<IActionResult> CreateShortageRequest(
            Guid campaignId,
            [FromBody] CreateSupplyShortageRequest request,
            CancellationToken cancellationToken)
            => Ok(await _reliefDistributionService.CreateShortageRequestAsync(campaignId, request, cancellationToken));

        [HttpGet("shortage-requests")]
        [Authorize(Roles = "Manager,Moderator,Volunteer")]
        public async Task<ActionResult<Pagination<SupplyShortageRequestResponse>>> GetShortageRequests(
            Guid campaignId,
            [FromQuery] SupplyShortageRequestQueryRequest request,
            CancellationToken cancellationToken)
            => Ok(await _reliefDistributionService.GetShortageRequestsAsync(campaignId, request, cancellationToken));

        [HttpPatch("shortage-requests/{shortageRequestId:guid}/approve")]
        [Authorize(Roles = "Manager,Moderator")]
        public async Task<IActionResult> ApproveShortageRequest(
            Guid campaignId,
            Guid shortageRequestId,
            [FromBody] ReviewSupplyShortageRequest request,
            CancellationToken cancellationToken)
            => Ok(await _reliefDistributionService.ApproveShortageRequestAsync(campaignId, shortageRequestId, request, cancellationToken));

        [HttpPatch("shortage-requests/{shortageRequestId:guid}/reject")]
        [Authorize(Roles = "Manager,Moderator")]
        public async Task<IActionResult> RejectShortageRequest(
            Guid campaignId,
            Guid shortageRequestId,
            [FromBody] ReviewSupplyShortageRequest request,
            CancellationToken cancellationToken)
            => Ok(await _reliefDistributionService.RejectShortageRequestAsync(campaignId, shortageRequestId, request, cancellationToken));
    }
}
