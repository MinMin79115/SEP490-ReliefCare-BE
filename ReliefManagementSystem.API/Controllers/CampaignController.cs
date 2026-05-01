using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ReliefManagementSystem.Application.Features.Campaign.Dtos.Requests;
using ReliefManagementSystem.Application.Interface;

namespace ReliefManagementSystem.API.Controllers
{
    [Route("api/campaigns")]
    [ApiController]
    public class CampaignController : ControllerBase
    {
        private readonly ICampaignService _campaignService;

        public CampaignController(ICampaignService campaignService)
        {
            _campaignService = campaignService;
        }

        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] CreateCampaignRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _campaignService.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = result.CampaignId }, result);
        }

        [HttpGet]
        public async Task<IActionResult> GetPaged(
            [FromQuery] CampaignListQueryRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _campaignService.GetPagedAsync(request, cancellationToken);
            return Ok(result);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            var result = await _campaignService.GetByIdAsync(id, cancellationToken);
            return Ok(result);
        }

        [HttpGet("{id:guid}/inventory-balance")]
        public async Task<IActionResult> GetInventoryBalance(Guid id, CancellationToken cancellationToken)
        {
            var result = await _campaignService.GetInventoryBalanceAsync(id, cancellationToken);
            return Ok(result);
        }

        [AllowAnonymous]
        [HttpGet("{id:guid}/summary")]
        public async Task<IActionResult> GetPublicSummary(Guid id, CancellationToken cancellationToken)
        {
            var result = await _campaignService.GetPublicSummaryAsync(id, cancellationToken);
            return Ok(result);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(
            Guid id,
            [FromBody] UpdateCampaignRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _campaignService.UpdateAsync(id, request, cancellationToken);
            return Ok(result);
        }

        [HttpPatch("{id:guid}/status")]
        public async Task<IActionResult> ChangeStatus(
            Guid id,
            [FromBody] ChangeCampaignStatusRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _campaignService.ChangeStatusAsync(id, request, cancellationToken);
            return Ok(result);
        }

        [HttpPost("{id:guid}/stations")]
        public async Task<IActionResult> AttachStation(
            Guid id,
            [FromBody] AttachCampaignStationRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _campaignService.AttachStationAsync(id, request, cancellationToken);
            return Ok(result);
        }

        [HttpDelete("{id:guid}/stations/{reliefStationId:guid}")]
        public async Task<IActionResult> DetachStation(
            Guid id,
            Guid reliefStationId,
            CancellationToken cancellationToken)
        {
            var result = await _campaignService.DetachStationAsync(id, reliefStationId, cancellationToken);
            return Ok(result);
        }

        [HttpPost("{id:guid}/extract-budget")]
        public async Task<IActionResult> ExtractBudget(
            Guid id,
            [FromBody] ExtractCampaignBudgetRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _campaignService.ExtractBudgetAsync(id, request, cancellationToken);
            return Ok(result);
        }

        [HttpGet("{id:guid}/extract-budget")]
        public async Task<IActionResult> GetExtractBudgetHistory(
            Guid id,
            [FromQuery] bool includeDeleted,
            CancellationToken cancellationToken)
        {
            var result = await _campaignService.GetBudgetTransferHistoryAsync(id, includeDeleted, cancellationToken);
            return Ok(result);
        }

        [HttpDelete("{id:guid}/extract-budget/{campaignBudgetTransferId:guid}")]
        public async Task<IActionResult> DeleteExtractBudgetHistory(
            Guid id,
            Guid campaignBudgetTransferId,
            CancellationToken cancellationToken)
        {
            await _campaignService.DeleteBudgetTransferHistoryAsync(id, campaignBudgetTransferId, cancellationToken);
            return NoContent();
        }

        [HttpPost("{id:guid}/teams")]
        [Authorize(Roles = "Manager,Moderator")]
        public async Task<IActionResult> AssignTeam(
            Guid id,
            [FromBody] AssignCampaignTeamRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _campaignService.AssignTeamAsync(id, request, cancellationToken);
            return Ok(result);
        }

        [HttpGet("{id:guid}/teams")]
        [Authorize(Roles = "Manager,Moderator,Volunteer")]
        public async Task<IActionResult> GetTeams(Guid id, CancellationToken cancellationToken)
        {
            var result = await _campaignService.GetTeamsAsync(id, cancellationToken);
            return Ok(result);
        }

        [HttpPatch("{id:guid}/teams/{campaignTeamId:guid}/status")]
        [Authorize(Roles = "Manager,Moderator")]
        public async Task<IActionResult> UpdateTeamStatus(
            Guid id,
            Guid campaignTeamId,
            [FromBody] UpdateCampaignTeamStatusRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _campaignService.UpdateTeamStatusAsync(id, campaignTeamId, request, cancellationToken);
            return Ok(result);
        }

        [HttpDelete("{id:guid}/teams/{campaignTeamId:guid}")]
        [Authorize(Roles = "Manager,Moderator")]
        public async Task<IActionResult> RemoveTeam(Guid id, Guid campaignTeamId, CancellationToken cancellationToken)
        {
            await _campaignService.RemoveTeamAsync(id, campaignTeamId, cancellationToken);
            return NoContent();
        }

        [HttpPost("{id:guid}/teams/{campaignTeamId:guid}/vehicles")]
        [Authorize(Roles = "Manager,Moderator")]
        public async Task<IActionResult> AssignVehicleToTeam(Guid id, Guid campaignTeamId, [FromBody] AssignCampaignVehicleRequest request, CancellationToken cancellationToken)
        {
            request.CampaignTeamId = campaignTeamId;
            var result = await _campaignService.AssignVehicleToTeamAsync(id, request, cancellationToken);
            return Ok(result);
        }

        [HttpGet("{id:guid}/vehicles")]
        [Authorize(Roles = "Manager,Moderator,Volunteer")]
        public async Task<IActionResult> GetCampaignVehicles(Guid id, [FromQuery] Guid? campaignTeamId, CancellationToken cancellationToken)
        {
            var result = await _campaignService.GetCampaignVehiclesAsync(id, campaignTeamId, cancellationToken);
            return Ok(result);
        }

        [HttpGet("{id:guid}/vehicles/my-assignment")]
        [Authorize(Roles = "Manager,Moderator,Volunteer")]
        public async Task<IActionResult> GetMyCampaignVehicleAssignment(Guid id, CancellationToken cancellationToken)
        {
            var result = await _campaignService.GetMyCampaignVehicleAssignmentAsync(id, cancellationToken);
            return Ok(result);
        }

        [HttpPatch("{id:guid}/vehicles/{campaignVehicleId:guid}")]
        [Authorize(Roles = "Manager,Moderator")]
        public async Task<IActionResult> UpdateCampaignVehicle(Guid id, Guid campaignVehicleId, [FromBody] UpdateCampaignVehicleAssignmentRequest request, CancellationToken cancellationToken)
        {
            var result = await _campaignService.UpdateCampaignVehicleAssignmentAsync(id, campaignVehicleId, request, cancellationToken);
            return Ok(result);
        }

        [HttpPatch("{id:guid}/vehicles/{campaignVehicleId:guid}/assign-driver")]
        [Authorize(Roles = "Manager,Moderator,Volunteer")]
        public async Task<IActionResult> AssignCampaignVehicleDriver(Guid id, Guid campaignVehicleId, [FromBody] AssignCampaignVehicleDriverRequest request, CancellationToken cancellationToken)
        {
            var result = await _campaignService.AssignCampaignVehicleDriverAsync(id, campaignVehicleId, request, cancellationToken);
            return Ok(result);
        }

        [HttpPatch("{id:guid}/vehicles/{campaignVehicleId:guid}/release")]
        [Authorize(Roles = "Manager,Moderator,Volunteer")]
        public async Task<IActionResult> ReleaseCampaignVehicle(Guid id, Guid campaignVehicleId, [FromBody] ReleaseCampaignVehicleRequest request, CancellationToken cancellationToken)
        {
            var result = await _campaignService.ReleaseCampaignVehicleAsync(id, campaignVehicleId, request, cancellationToken);
            return Ok(result);
        }

        [HttpPatch("{id:guid}/vehicles/{campaignVehicleId:guid}/handoff")]
        [Authorize(Roles = "Manager,Moderator,Volunteer")]
        public async Task<IActionResult> HandoffCampaignVehicle(Guid id, Guid campaignVehicleId, [FromBody] HandoffCampaignVehicleRequest request, CancellationToken cancellationToken)
        {
            var result = await _campaignService.HandoffCampaignVehicleAsync(id, campaignVehicleId, request, cancellationToken);
            return Ok(result);
        }

        [HttpPatch("{id:guid}/vehicles/{campaignVehicleId:guid}/return-to-coordinator")]
        [Authorize(Roles = "Manager,Moderator,Volunteer")]
        public async Task<IActionResult> ReturnCampaignVehicleToCoordinator(Guid id, Guid campaignVehicleId, [FromBody] ReturnCampaignVehicleToCoordinatorRequest request, CancellationToken cancellationToken)
        {
            var result = await _campaignService.ReturnCampaignVehicleToCoordinatorAsync(id, campaignVehicleId, request, cancellationToken);
            return Ok(result);
        }

        [HttpDelete("{id:guid}/vehicles/{campaignVehicleId:guid}")]
        [Authorize(Roles = "Manager,Moderator")]
        public async Task<IActionResult> RemoveCampaignVehicle(Guid id, Guid campaignVehicleId, CancellationToken cancellationToken)
        {
            await _campaignService.RemoveCampaignVehicleAssignmentAsync(id, campaignVehicleId, cancellationToken);
            return NoContent();
        }

        [Authorize]
        [HttpPost("{id:guid}/volunteer-registrations")]
        public async Task<IActionResult> RegisterVolunteer(Guid id, CancellationToken cancellationToken)
        {
            var result = await _campaignService.RegisterVolunteerAsync(id, cancellationToken);
            return Ok(result);
        }

        [Authorize]
        [HttpDelete("{id:guid}/volunteer-registrations/me")]
        public async Task<IActionResult> CancelVolunteerRegistration(Guid id, CancellationToken cancellationToken)
        {
            await _campaignService.CancelVolunteerRegistrationAsync(id, cancellationToken);
            return NoContent();
        }

        [HttpGet("{id:guid}/volunteer-registrations")]
        public async Task<IActionResult> GetVolunteerRegistrations(Guid id, CancellationToken cancellationToken)
        {
            var result = await _campaignService.GetVolunteerRegistrationsAsync(id, cancellationToken);
            return Ok(result);
        }
    }
}
