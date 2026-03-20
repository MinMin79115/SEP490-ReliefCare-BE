using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReliefManagementSystem.Application.Common.Models;
using ReliefManagementSystem.Application.Features.ReliefStation.Dtos;
using ReliefManagementSystem.Application.Features.ReliefStation.DTOs.Request;
using ReliefManagementSystem.Application.Features.ReliefStation.DTOs.Response;
using ReliefManagementSystem.Application.Interface;
using ReliefManagementSystem.Domain.Enum;
using Swashbuckle.AspNetCore.Annotations;

namespace ReliefManagementSystem.API.Controllers
{
    /// <summary>
    /// Quản lý trạm cứu trợ (ReliefStation) và phân công Team.
    /// </summary>
    [Route("api/relief-stations")]
    [ApiController]
    [Authorize]
    public class ReliefStationController : ControllerBase
    {
        private readonly IReliefStationService _stationService;

        public ReliefStationController(IReliefStationService stationService)
        {
            _stationService = stationService;
        }
        [Authorize(Roles = "Manager")]
        [SwaggerOperation(OperationId = "CreateReliefStation", Description = "Manager tạo trạm mới cho tỉnh")]
        [HttpPost("provincial")]
        public async Task<IActionResult> CreateProvincialStation(
            CreateProvincialReliefStationRequest request, CancellationToken cancellationToken)
        {
            var result = await _stationService.CreateProvincialReliefStationAsync(request,cancellationToken);
            return Ok(result);
        }

        [Authorize(Roles = "Manager")]
        [SwaggerOperation(OperationId = "UpdateProvincialStation", Description = "Manager cập nhật thông tin trạm cấp Tỉnh")]
        [HttpPut("provincial/{stationId:guid}")]
        public async Task<IActionResult> UpdateProvincialStation(
            Guid stationId,
            [FromBody] UpdateProvincialStationRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _stationService.UpdateProvincialReliefStationAsync(stationId, request, cancellationToken);
            return Ok(result);
        }

        [SwaggerOperation(OperationId = "GetProvincialStations", Description = "Lấy danh sách trạm cấp Tỉnh, có hỗ trợ tìm kiếm và phân trang")]
        [HttpGet("provincial")]
        public async Task<IActionResult> GetProvincialStations(
            [FromQuery] string? search,
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var (items, totalCount) = await _stationService.GetProvincialStationsAsync(search, pageIndex, pageSize, cancellationToken);
            return Ok(new
            {
                TotalCount = totalCount,
                PageIndex = pageIndex,
                PageSize = pageSize,
                Items = items
            });
        }

        [Authorize(Roles = "Moderator")]
        [SwaggerOperation(OperationId = "GetCurrentModeratorStation", Description = "Lấy thông tin trạm hiện tại Moderator đang quản lý")]
        [HttpGet("my-station")]
        public async Task<IActionResult> GetCurrentModeratorStation(CancellationToken cancellationToken)
        {
            var result = await _stationService.GetCurrentModeratorStationAsync(cancellationToken);
            return Ok(result);
        }

        [Authorize(Roles = "Manager")]
        [SwaggerOperation(OperationId = "DisableProvincialStation", Description = "Manager huỷ (vô hiệu hoá) trạm cấp Tỉnh và các kho liên quan")]
        [HttpPut("provincial/{stationId:guid}/disable")]
        public async Task<IActionResult> DisableProvincialStation(Guid stationId, CancellationToken cancellationToken)
        {
            var result = await _stationService.DisableProvincialStationAsync(stationId, cancellationToken);
            return Ok(result);
        }

        [Authorize(Roles = "Manager")]
        [SwaggerOperation(OperationId = "ActivateProvincialStation", Description = "Manager kích hoạt lại trạm cấp Tỉnh và các kho liên quan")]
        [HttpPut("provincial/{stationId:guid}/activate")]
        public async Task<IActionResult> ActivateProvincialStation(Guid stationId, CancellationToken cancellationToken)
        {
            var result = await _stationService.ActivateProvincialStationAsync(stationId, cancellationToken);
            return Ok(result);
        }

        [Authorize(Roles = "Manager")]
        [SwaggerOperation(OperationId = "AssignModerator", Description = "Manager gán 1 Moderator duy nhất (làm trưởng trạm) cho một trạm cứu trợ")]
        [HttpPut("{stationId:guid}/assign-moderator")]
        public async Task<IActionResult> AssignModerator(Guid stationId, [FromBody] AssignModeratorRequest request, CancellationToken cancellationToken)
        {
            await _stationService.AssignModeratorAsync(stationId, request, cancellationToken);
            return NoContent();
        }

        [Authorize(Roles = "Moderator")]
        [SwaggerOperation(OperationId = "AssignTeamToStation", Description = "Moderator trưởng trạm duyệt/gán team vào trạm")]
        [HttpPost("{stationId:guid}/teams")]
        public async Task<IActionResult> AssignTeamToStation(
            Guid stationId,
            [FromBody] AssignTeamRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _stationService.AssignTeamToStationAsync(stationId, request, cancellationToken);
            return Ok(result);
        }

        [Authorize(Roles = "Moderator")]
        [SwaggerOperation(OperationId = "UpdateTeamAssignmentStatus", Description = "Moderator trưởng trạm cập nhật trạng thái team tại trạm (Active/Transferred/Suspended/Completed/Cancelled)")]
        [HttpPatch("{stationId:guid}/teams/{teamId:guid}/status")]
        public async Task<IActionResult> UpdateTeamAssignmentStatus(
            Guid stationId,
            Guid teamId,
            [FromBody] UpdateTeamAssignmentRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _stationService.UpdateTeamAssignmentStatusAsync(stationId, teamId, request, cancellationToken);
            return Ok(result);
        }

    }
}
