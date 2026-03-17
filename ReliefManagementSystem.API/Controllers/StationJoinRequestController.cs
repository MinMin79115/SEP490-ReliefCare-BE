using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReliefManagementSystem.Application.Features.StationJoinRequest.DTOs.Request;
using ReliefManagementSystem.Application.Interface;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

namespace ReliefManagementSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class StationJoinRequestController : ControllerBase
    {
        private readonly IStationJoinRequestService _service;

        public StationJoinRequestController(IStationJoinRequestService service)
        {
            _service = service;
        }

        [HttpPost]
        [Authorize(Roles = "Volunteer")]
        [SwaggerOperation(OperationId = "CreateStationJoinRequest", Description = "Team leader tạo yêu cầu xin team vào trạm")]
        public async Task<IActionResult> CreateRequest([FromBody] CreateStationJoinRequestRequest request, CancellationToken cancellationToken)
        {
            var leaderId = GetCurrentUserId();
            var result = await _service.CreateRequestAsync(request, leaderId, cancellationToken);
            return Ok(result);
        }

        [HttpGet("{id:guid}")]
        [SwaggerOperation(OperationId = "GetStationJoinRequestById", Description = "Lấy chi tiết yêu cầu xin vào trạm")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            var result = await _service.GetByIdAsync(id, cancellationToken);
            return Ok(result);
        }

        [HttpGet("my-requests")]
        [Authorize(Roles = "Volunteer")]
        [SwaggerOperation(OperationId = "GetMyStationJoinRequests", Description = "Leader xem các yêu cầu đã gửi vào trạm")]
        public async Task<IActionResult> GetMyRequests(
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var leaderId = GetCurrentUserId();
            var result = await _service.GetMyRequestsAsync(leaderId, pageIndex, pageSize, cancellationToken);
            return Ok(result);
        }

        [HttpPatch("{id:guid}/cancel")]
        [Authorize(Roles = "Volunteer")]
        [SwaggerOperation(OperationId = "CancelStationJoinRequest", Description = "Leader hủy yêu cầu xin vào trạm khi đang pending")]
        public async Task<IActionResult> Cancel(Guid id, CancellationToken cancellationToken)
        {
            var leaderId = GetCurrentUserId();
            await _service.CancelAsync(id, leaderId, cancellationToken);
            return NoContent();
        }

        [HttpGet("station/{stationId:guid}/pending")]
        [Authorize(Roles = "Moderator")]
        [SwaggerOperation(OperationId = "GetPendingStationJoinRequests", Description = "Trưởng trạm xem danh sách request pending")]
        public async Task<IActionResult> GetPendingByStation(
            Guid stationId,
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var moderatorId = GetCurrentUserId();
            var result = await _service.GetPendingByStationAsync(stationId, moderatorId, pageIndex, pageSize, cancellationToken);
            return Ok(result);
        }

        [HttpPatch("{id:guid}/approve")]
        [Authorize(Roles = "Moderator")]
        [SwaggerOperation(OperationId = "ApproveStationJoinRequest", Description = "Trưởng trạm duyệt yêu cầu xin vào trạm")]
        public async Task<IActionResult> Approve(
            Guid id,
            [FromBody] ReviewStationJoinRequestRequest request,
            CancellationToken cancellationToken)
        {
            var moderatorId = GetCurrentUserId();
            var result = await _service.ApproveAsync(id, moderatorId, request, cancellationToken);
            return Ok(result);
        }

        [HttpPatch("{id:guid}/reject")]
        [Authorize(Roles = "Moderator")]
        [SwaggerOperation(OperationId = "RejectStationJoinRequest", Description = "Trưởng trạm từ chối yêu cầu xin vào trạm kèm lý do")]
        public async Task<IActionResult> Reject(
            Guid id,
            [FromBody] ReviewStationJoinRequestRequest request,
            CancellationToken cancellationToken)
        {
            var moderatorId = GetCurrentUserId();
            var result = await _service.RejectAsync(id, moderatorId, request, cancellationToken);
            return Ok(result);
        }

        private Guid GetCurrentUserId()
        {
            return Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
        }
    }
}
