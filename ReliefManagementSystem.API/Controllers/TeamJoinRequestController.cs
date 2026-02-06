using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReliefManagementSystem.Application.Features.TeamJoinRequest.DTOs.Request;
using ReliefManagementSystem.Application.Interface;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

namespace ReliefManagementSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TeamJoinRequestController : ControllerBase
    {
        private readonly ITeamJoinRequestService _service;

        public TeamJoinRequestController(ITeamJoinRequestService service)
        {
            _service = service;
        }

        // POST /api/team-join-request
        [HttpPost]
        [Authorize(Roles = "Volunteer")]
        [SwaggerOperation(OperationId = "CreateTeamJoinRequest", Description = "Volunteer tạo yêu cầu tham gia team")]
        public async Task<IActionResult> CreateRequest([FromBody] CreateTeamJoinRequest request, CancellationToken cancellationToken)
        {
            var volunteerId = GetCurrentUserId();
            var result = await _service.CreateRequestAsync(request, volunteerId, cancellationToken);
            return Ok(result);
        }

        // GET /api/team-join-request/{id}
        [HttpGet("{id:guid}")]
        [SwaggerOperation(OperationId = "GetTeamJoinRequestById", Description = "Lấy chi tiết yêu cầu tham gia team")]
        public async Task<IActionResult> GetRequestById(Guid id, CancellationToken cancellationToken)
        {
            var result = await _service.GetRequestByIdAsync(id, cancellationToken);
            return Ok(result);
        }

        // GET /api/team-join-request/my-requests
        [HttpGet("my-requests")]
        [Authorize(Roles = "Volunteer")]
        [SwaggerOperation(OperationId = "GetMyTeamJoinRequests", Description = "Volunteer xem danh sách yêu cầu của mình và trạng thái")]
        public async Task<IActionResult> GetMyRequests(CancellationToken cancellationToken)
        {
            var volunteerId = GetCurrentUserId();
            var result = await _service.GetMyRequestsAsync(volunteerId, cancellationToken);
            return Ok(result);
        }

        // PATCH /api/team-join-request/{id}/cancel
        [HttpPatch("{id:guid}/cancel")]
        [Authorize(Roles = "Volunteer")]
        [SwaggerOperation(OperationId = "CancelTeamJoinRequest", Description = "Volunteer hủy yêu cầu tham gia team")]
        public async Task<IActionResult> CancelRequest(Guid id, CancellationToken cancellationToken)
        {
            var volunteerId = GetCurrentUserId();
            await _service.CancelRequestAsync(id, volunteerId, cancellationToken);
            return NoContent();
        }

        // POST /api/team-join-request/{id}/review
        //[HttpPost("{id:guid}/review")]
        //[Authorize(Roles = "Moderator")]
        //public async Task<IActionResult> ReviewRequest(Guid id, [FromBody] ReviewTeamJoinRequest request, CancellationToken cancellationToken)
        //{
        //    var moderatorId = GetCurrentUserId();
        //    var result = await _service.ReviewRequestAsync(id, request, moderatorId, cancellationToken);
        //    return Ok(result);
        //}

        // PATCH /api/team-join-request/{id}/approve
        [HttpPatch("{id:guid}/approve")]
        [Authorize(Roles = "Moderator")]
        [SwaggerOperation(OperationId = "ApproveTeamJoinRequest", Description = "Moderator duyệt yêu cầu tham gia team. Update ApprovedAt, ApprovedBy, ReviewNote và Status")]
        public async Task<IActionResult> ApproveRequest(
            Guid id, 
            [FromBody] ReviewTeamJoinRequest request, 
            CancellationToken cancellationToken)
        {
            var moderatorId = GetCurrentUserId();
            var result = await _service.ApproveRequestAsync(id, moderatorId, request, cancellationToken);
            return Ok(result);
        }

        // PATCH /api/team-join-request/{id}/reject
        [HttpPatch("{id:guid}/reject")]
        [Authorize(Roles = "Moderator")]
        [SwaggerOperation(OperationId = "RejectTeamJoinRequest", Description = "Moderator từ chối yêu cầu tham gia team. Update RejectedAt, RejectedBy, ReviewNote và Status")]
        public async Task<IActionResult> RejectRequest(
            Guid id, 
            [FromBody] ReviewTeamJoinRequest request, 
            CancellationToken cancellationToken)
        {
            var moderatorId = GetCurrentUserId();
            var result = await _service.RejectRequestAsync(id, moderatorId, request, cancellationToken);
            return Ok(result);
        }

        // GET /api/team-join-request/team/{teamId}
        [HttpGet("team/{teamId:guid}")]
        [Authorize(Roles = "Moderator")]
        [SwaggerOperation(OperationId = "GetTeamJoinRequestsByTeam", Description = "Moderator xem yêu cầu tham gia của volunteers có skills xin vào team của mình")]
        public async Task<IActionResult> GetRequestsByTeam(Guid teamId, CancellationToken cancellationToken)
        {
            var moderatorId = GetCurrentUserId();
            var result = await _service.GetRequestsByTeamAsync(teamId, moderatorId, cancellationToken);
            return Ok(result);
        }

        // // GET /api/team-join-request/my-pending
        // [HttpGet("my-pending")]
        // [Authorize(Roles = "Moderator")]
        // public async Task<IActionResult> GetMyPendingRequests(CancellationToken cancellationToken)
        // {
        //     var moderatorId = GetCurrentUserId();
        //     var result = await _service.GetPendingRequestsForMyTeamsAsync(moderatorId, cancellationToken);
        //     return Ok(result);
        // }

        // // GET /api/team-join-request/team/5
        // [HttpGet("team/{teamId:guid}")]
        // [Authorize(Roles = "Moderator")]
        // public async Task<IActionResult> GetRequestsByTeam(Guid teamId, CancellationToken cancellationToken)
        // {
        //     var moderatorId = GetCurrentUserId();
        //     var result = await _service.GetRequestsByTeamAsync(teamId, moderatorId, cancellationToken);
        //     return Ok(result);
        // }

        private Guid GetCurrentUserId()
        {
            return Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
        }
    }
}
