using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReliefManagementSystem.Application.Features.Relief.DTOs.Request;
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
        public async Task<IActionResult> ImportHouseholds(
            Guid campaignId,
            [FromBody] ImportCampaignHouseholdsRequest request,
            CancellationToken cancellationToken)
            => Ok(await _reliefDistributionService.ImportCampaignHouseholdsAsync(campaignId, request, cancellationToken));

        [HttpPatch("households/{campaignHouseholdId:guid}/assign")]
        public async Task<IActionResult> AssignHousehold(
            Guid campaignId,
            Guid campaignHouseholdId,
            [FromBody] AssignHouseholdRequest request,
            CancellationToken cancellationToken)
            => Ok(await _reliefDistributionService.AssignHouseholdAsync(campaignId, campaignHouseholdId, request, cancellationToken));

        [HttpGet("households")]
        public async Task<IActionResult> GetHouseholds(
            Guid campaignId,
            [FromQuery] HouseholdFulfillmentStatus? status,
            CancellationToken cancellationToken)
            => Ok(await _reliefDistributionService.GetCampaignHouseholdsAsync(campaignId, status, cancellationToken));

        [HttpGet("checklist")]
        public async Task<IActionResult> GetChecklist(
            Guid campaignId,
            [FromQuery] Guid? campaignTeamId,
            [FromQuery] HouseholdFulfillmentStatus? status,
            CancellationToken cancellationToken)
            => Ok(await _reliefDistributionService.GetChecklistAsync(campaignId, campaignTeamId, status, cancellationToken));

        [HttpPost("distribution-points")]
        public async Task<IActionResult> CreateDistributionPoint(
            Guid campaignId,
            [FromBody] CreateDistributionPointRequest request,
            CancellationToken cancellationToken)
            => Ok(await _reliefDistributionService.CreateDistributionPointAsync(campaignId, request, cancellationToken));

        [HttpGet("distribution-points")]
        public async Task<IActionResult> GetDistributionPoints(Guid campaignId, CancellationToken cancellationToken)
            => Ok(await _reliefDistributionService.GetDistributionPointsAsync(campaignId, cancellationToken));

        [HttpPost("packages")]
        public async Task<IActionResult> CreateReliefPackage(
            Guid campaignId,
            [FromBody] CreateReliefPackageDefinitionRequest request,
            CancellationToken cancellationToken)
            => Ok(await _reliefDistributionService.CreateReliefPackageDefinitionAsync(campaignId, request, cancellationToken));

        [HttpGet("packages")]
        public async Task<IActionResult> GetReliefPackages(Guid campaignId, CancellationToken cancellationToken)
            => Ok(await _reliefDistributionService.GetReliefPackageDefinitionsAsync(campaignId, cancellationToken));

        [HttpPost("deliveries/{householdDeliveryId:guid}/complete")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> CompleteDelivery(
            Guid campaignId,
            Guid householdDeliveryId,
            [FromForm] CompleteHouseholdDeliveryRequest request,
            [FromForm] IFormFile proofImage,
            CancellationToken cancellationToken)
            => Ok(await _reliefDistributionService.CompleteHouseholdDeliveryAsync(campaignId, householdDeliveryId, request, proofImage, cancellationToken));

        [HttpPost("shortage-requests")]
        public async Task<IActionResult> CreateShortageRequest(
            Guid campaignId,
            [FromBody] CreateSupplyShortageRequest request,
            CancellationToken cancellationToken)
            => Ok(await _reliefDistributionService.CreateShortageRequestAsync(campaignId, request, cancellationToken));

        [HttpGet("shortage-requests")]
        public async Task<IActionResult> GetShortageRequests(
            Guid campaignId,
            [FromQuery] SupplyShortageRequestStatus? status,
            CancellationToken cancellationToken)
            => Ok(await _reliefDistributionService.GetShortageRequestsAsync(campaignId, status, cancellationToken));

        [HttpPatch("shortage-requests/{shortageRequestId:guid}/approve")]
        public async Task<IActionResult> ApproveShortageRequest(
            Guid campaignId,
            Guid shortageRequestId,
            [FromBody] ReviewSupplyShortageRequest request,
            CancellationToken cancellationToken)
            => Ok(await _reliefDistributionService.ApproveShortageRequestAsync(campaignId, shortageRequestId, request, cancellationToken));

        [HttpPatch("shortage-requests/{shortageRequestId:guid}/reject")]
        public async Task<IActionResult> RejectShortageRequest(
            Guid campaignId,
            Guid shortageRequestId,
            [FromBody] ReviewSupplyShortageRequest request,
            CancellationToken cancellationToken)
            => Ok(await _reliefDistributionService.RejectShortageRequestAsync(campaignId, shortageRequestId, request, cancellationToken));
    }
}
