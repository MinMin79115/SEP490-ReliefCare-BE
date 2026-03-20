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

        [HttpPost("{id:guid}/teams")]
        public async Task<IActionResult> AssignTeam(
            Guid id,
            [FromBody] AssignCampaignTeamRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _campaignService.AssignTeamAsync(id, request, cancellationToken);
            return Ok(result);
        }

        [HttpGet("{id:guid}/teams")]
        public async Task<IActionResult> GetTeams(Guid id, CancellationToken cancellationToken)
        {
            var result = await _campaignService.GetTeamsAsync(id, cancellationToken);
            return Ok(result);
        }

        [HttpPatch("{id:guid}/teams/{teamId:guid}/status")]
        public async Task<IActionResult> UpdateTeamStatus(
            Guid id,
            Guid teamId,
            [FromBody] UpdateCampaignTeamStatusRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _campaignService.UpdateTeamStatusAsync(id, teamId, request, cancellationToken);
            return Ok(result);
        }

        [HttpDelete("{id:guid}/teams/{teamId:guid}")]
        public async Task<IActionResult> RemoveTeam(Guid id, Guid teamId, CancellationToken cancellationToken)
        {
            await _campaignService.RemoveTeamAsync(id, teamId, cancellationToken);
            return NoContent();
        }
    }
}
