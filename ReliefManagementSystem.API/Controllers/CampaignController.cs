using Microsoft.AspNetCore.Mvc;
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
    }
}
