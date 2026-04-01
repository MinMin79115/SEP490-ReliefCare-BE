using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReliefManagementSystem.Application.Features.ReliefFulfillment.DTOs.Request;
using ReliefManagementSystem.Application.Interface;

namespace ReliefManagementSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ReliefFulfillmentController : ControllerBase
    {
        private readonly IReliefFulfillmentService _reliefFulfillmentService;

        public ReliefFulfillmentController(IReliefFulfillmentService reliefFulfillmentService)
        {
            _reliefFulfillmentService = reliefFulfillmentService;
        }

        [HttpPost("/api/distributionsession/{id:guid}/fulfillments")]
        [Authorize(Roles = "Volunteer,Moderator,Manager,Admin")]
        public async Task<IActionResult> Create(Guid id, [FromBody] CreateReliefFulfillmentRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _reliefFulfillmentService.CreateAsync(id, request, cancellationToken);
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

        [HttpGet("by-request/{requestId:guid}")]
        public async Task<IActionResult> GetByRequest(Guid requestId, CancellationToken cancellationToken)
        {
            var result = await _reliefFulfillmentService.GetByRequestAsync(requestId, cancellationToken);
            return Ok(result);
        }

        [HttpGet("by-session/{sessionId:guid}")]
        public async Task<IActionResult> GetBySession(Guid sessionId, CancellationToken cancellationToken)
        {
            var result = await _reliefFulfillmentService.GetBySessionAsync(sessionId, cancellationToken);
            return Ok(result);
        }

        [HttpPost("{id:guid}/proof")]
        [Authorize(Roles = "Volunteer,Moderator,Manager,Admin")]
        public async Task<IActionResult> AddProof(Guid id, [FromBody] UpdateReliefFulfillmentProofRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _reliefFulfillmentService.AddProofAsync(id, request, cancellationToken);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPost("{id:guid}/failed")]
        [Authorize(Roles = "Volunteer,Moderator,Manager,Admin")]
        public async Task<IActionResult> MarkFailed(Guid id, [FromBody] MarkReliefFulfillmentFailedRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _reliefFulfillmentService.MarkFailedAsync(id, request, cancellationToken);
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
