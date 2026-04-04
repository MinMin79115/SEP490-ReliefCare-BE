using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReliefManagementSystem.Application.Features.DistributionSession.DTOs.Request;
using ReliefManagementSystem.Application.Interface;

namespace ReliefManagementSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Moderator,Manager,Admin")]
    public class DistributionSessionController : ControllerBase
    {
        private readonly IDistributionSessionService _distributionSessionService;

        public DistributionSessionController(IDistributionSessionService distributionSessionService)
        {
            _distributionSessionService = distributionSessionService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateDistributionSessionRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _distributionSessionService.CreateAsync(request, cancellationToken);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Search(
            [FromQuery] string? search,
            [FromQuery] int? statusFilter,
            [FromQuery] Guid? campaignId,
            [FromQuery] Guid? reliefStationId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var result = await _distributionSessionService.SearchAsync(new SearchDistributionSessionRequest
            {
                Search = search,
                StatusFilter = statusFilter,
                CampaignId = campaignId,
                ReliefStationId = reliefStationId,
                PageNumber = pageNumber,
                PageSize = pageSize
            }, cancellationToken);

            return Ok(result);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _distributionSessionService.GetByIdAsync(id, cancellationToken);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPost("{id:guid}/items")]
        public async Task<IActionResult> AddItems(Guid id, [FromBody] AddDistributionSessionItemsRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _distributionSessionService.AddItemsAsync(id, request, cancellationToken);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("{id:guid}/requests")]
        public async Task<IActionResult> AttachRequests(Guid id, [FromBody] AttachRequestsToSessionRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _distributionSessionService.AttachRequestsAsync(id, request, cancellationToken);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("{id:guid}/ready")]
        public async Task<IActionResult> MarkReady(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _distributionSessionService.MarkReadyAsync(id, cancellationToken);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("{id:guid}/start")]
        public async Task<IActionResult> Start(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _distributionSessionService.StartAsync(id, cancellationToken);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("{id:guid}/complete")]
        public async Task<IActionResult> Complete(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _distributionSessionService.CompleteAsync(id, cancellationToken);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("{id:guid}/cancel")]
        public async Task<IActionResult> Cancel(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _distributionSessionService.CancelAsync(id, cancellationToken);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
