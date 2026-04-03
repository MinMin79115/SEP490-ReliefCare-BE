using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using ReliefManagementSystem.Application.Features.ReliefRequest.DTOs.Request;
using ReliefManagementSystem.Application.Interface;
using Swashbuckle.AspNetCore.Annotations;

namespace ReliefManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReliefRequestController : ControllerBase
    {
        private readonly IReliefRequestService _reliefRequestService;

        public ReliefRequestController(IReliefRequestService reliefRequestService)
        {
            _reliefRequestService = reliefRequestService;
        }

        [HttpPost]
        [AllowAnonymous]
        [EnableRateLimiting("rescue-create")]
        [SwaggerOperation(Summary = "Tạo yêu cầu cứu trợ mới")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<IActionResult> CreateReliefRequest([FromBody] CreateReliefRequestDto request, CancellationToken cancellationToken = default)
        {
            var result = await _reliefRequestService.CreateReliefRequestAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetReliefRequestById), new { id = result.RequestId }, result);
        }

        [HttpGet]
        [SwaggerOperation(Summary = "Lấy danh sách relief request")]
        public async Task<IActionResult> SearchReliefRequests(
            [FromQuery] string? search,
            [FromQuery] int? statusFilter,
            [FromQuery] Guid? assignedStationId,
            [FromQuery] Guid? campaignId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var result = await _reliefRequestService.SearchReliefRequestsAsync(new SearchReliefRequestDto
            {
                Search = search,
                StatusFilter = statusFilter,
                AssignedStationId = assignedStationId,
                CampaignId = campaignId,
                PageNumber = pageNumber,
                PageSize = pageSize
            }, cancellationToken);

            return Ok(result);
        }

        [HttpGet("{id}")]
        [SwaggerOperation(Summary = "Lấy chi tiết relief request")]
        public async Task<IActionResult> GetReliefRequestById(Guid id, CancellationToken cancellationToken = default)
        {
            var result = await _reliefRequestService.GetReliefRequestByIdAsync(id, cancellationToken);
            return Ok(result);
        }

        [HttpGet("stats")]
        [Authorize(Roles = "Moderator,Manager,Admin")]
        [SwaggerOperation(Summary = "Lấy thống kê relief request")]
        public async Task<IActionResult> GetStats(
            [FromQuery] Guid? campaignId,
            [FromQuery] Guid? assignedStationId,
            CancellationToken cancellationToken = default)
        {
            var result = await _reliefRequestService.GetStatsAsync(campaignId, assignedStationId, cancellationToken);
            return Ok(result);
        }

        [HttpPost("{id}/verify")]
        [Authorize(Roles = "Moderator,Manager,Admin")]
        public async Task<IActionResult> VerifyReliefRequest(Guid id, [FromBody] VerifyReliefRequestDto request, CancellationToken cancellationToken = default)
        {
            var result = await _reliefRequestService.VerifyReliefRequestAsync(id, request, cancellationToken);
            return Ok(result);
        }

        [HttpPost("{id}/approve")]
        [Authorize(Roles = "Moderator,Manager,Admin")]
        public async Task<IActionResult> ApproveReliefRequest(Guid id, [FromBody] ApproveReliefRequestDto request, CancellationToken cancellationToken = default)
        {
            var result = await _reliefRequestService.ApproveReliefRequestAsync(id, request, cancellationToken);
            return Ok(result);
        }

        [HttpPost("{id}/reject")]
        [Authorize(Roles = "Moderator,Manager,Admin")]
        public async Task<IActionResult> RejectReliefRequest(Guid id, [FromBody] RejectReliefRequestDto request, CancellationToken cancellationToken = default)
        {
            var result = await _reliefRequestService.RejectReliefRequestAsync(id, request, cancellationToken);
            return Ok(result);
        }

        [HttpPost("{id}/assign-station")]
        [Authorize(Roles = "Moderator,Manager,Admin")]
        public async Task<IActionResult> AssignStation(Guid id, [FromBody] AssignReliefRequestStationDto request, CancellationToken cancellationToken = default)
        {
            var result = await _reliefRequestService.AssignStationAsync(id, request, cancellationToken);
            return Ok(result);
        }

        [HttpPost("{id}/assign-campaign")]
        [Authorize(Roles = "Moderator,Manager,Admin")]
        public async Task<IActionResult> AssignCampaign(Guid id, [FromBody] AssignReliefRequestCampaignDto request, CancellationToken cancellationToken = default)
        {
            var result = await _reliefRequestService.AssignCampaignAsync(id, request, cancellationToken);
            return Ok(result);
        }

        [HttpPost("{id}/complete")]
        [Authorize(Roles = "Moderator,Manager,Admin")]
        public async Task<IActionResult> Complete(Guid id, [FromBody] CompleteReliefRequestDto request, CancellationToken cancellationToken = default)
        {
            var result = await _reliefRequestService.CompleteAsync(id, request, cancellationToken);
            return Ok(result);
        }
    }
}
