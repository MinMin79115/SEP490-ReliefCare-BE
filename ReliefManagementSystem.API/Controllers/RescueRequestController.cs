using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ReliefManagementSystem.Application.Common.Interface;
using ReliefManagementSystem.Application.Features.RescueRequest.DTOs.Request;
using ReliefManagementSystem.Application.Features.RescueRequest.DTOs.Response;
using ReliefManagementSystem.Application.Interface;
using ReliefManagementSystem.Application.Services;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ReliefManagementSystem.API.Controllers
{
    /// <summary>Controller for rescue request operations</summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class RescueRequestController : ControllerBase
    {
        private readonly IRescueRequestService _rescueRequestService;
        private readonly IWeatherService _weatherService;

        public RescueRequestController(IRescueRequestService rescueRequestService, IWeatherService weatherService)
        {
            _rescueRequestService = rescueRequestService;
            _weatherService = weatherService;
        }

        /// <summary>
        /// Gửi yêu cầu cứu hộ mới
        /// - POST /api/rescuerequest
        /// - Tự động tính priority và dispatch (nếu Emergency type)
        /// - Người dùng không bắt buộc phải đăng nhập (AllowAnonymous)
        /// </summary>
        [HttpPost]
        [AllowAnonymous]
        [SwaggerOperation(
            OperationId = "CreateRescueRequest",
            Summary = "Tạo yêu cầu cứu hộ mới",
            Description = "Người dùng/khách gửi yêu cầu cứu hộ. Hệ thống tự động gắn campaign Rescue đang hiệu lực, chọn trạm gần nhất theo Goong + bán kính CoverageRadiusKm, tạo operation dispatch và tạo RequestVerification. Với Emergency sẽ auto-check thời tiết để quyết định trạng thái xác minh ban đầu.")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CreateRescueRequest(
            [FromBody] CreateRescueRequestDto request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _rescueRequestService.CreateRescueRequestAsync(request, cancellationToken);
                return CreatedAtAction(nameof(GetRescueRequestById), new { id = result.RequestId }, result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new
                {
                    statusCode = StatusCodes.Status401Unauthorized,
                    message = ex.Message,
                    traceId = HttpContext.TraceIdentifier
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new
                {
                    statusCode = StatusCodes.Status400BadRequest,
                    message = ex.Message,
                    traceId = HttpContext.TraceIdentifier
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    statusCode = StatusCodes.Status500InternalServerError,
                    message = "An error occurred while processing your request",
                    detail = ex.Message,
                    traceId = HttpContext.TraceIdentifier
                });
            }
        }

        /// <summary>
        /// Tìm kiếm danh sách yêu cầu cứu hộ
        /// - GET /api/rescuerequest
        /// </summary>
        [HttpGet]
        [SwaggerOperation(
            OperationId = "SearchRescueRequests",
            Summary = "Lấy danh sách rescue request có phân trang + tìm kiếm",
            Description = "Search theo: ReporterFullName, ReporterPhone, Address, Description. Có thể lọc thêm theo statusFilter (int) và phân trang bằng pageNumber/pageSize.")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> SearchRescueRequests(
            [FromQuery] string? search,
            [FromQuery] int? statusFilter,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var result = await _rescueRequestService.SearchRescueRequestsAsync(
                new SearchRescueRequestDto
                {
                    Search = search,
                    StatusFilter = statusFilter,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                },
                cancellationToken);

            return Ok(result);
        }

        [HttpGet("probe-distance-matrix")]
        [AllowAnonymous]
        [SwaggerOperation(
            OperationId = "ProbeGoongDistanceMatrix",
            Summary = "Test Goong Distance Matrix",
            Description = "API probe để kiểm tra khoảng cách/ETA từ origin tới danh sách destination bằng Goong API key hiện tại.")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ProbeDistanceMatrix(
            [FromQuery] double originLat,
            [FromQuery] double originLng,
            [FromQuery] List<double> destinationLats,
            [FromQuery] List<double> destinationLngs,
            CancellationToken cancellationToken = default)
        {
            var result = await _rescueRequestService.ProbeDistanceMatrixAsync(
                originLat,
                originLng,
                destinationLats,
                destinationLngs,
                cancellationToken);

            return Ok(result);
        }

        /// <summary>
        /// Lấy thời tiết hiện tại theo tọa độ + điểm rủi ro thời tiết
        /// - GET /api/rescuerequest/probe-weather?lat=...&lng=...
        /// </summary>
        [HttpGet("probe-weather")]
        [AllowAnonymous]
        [SwaggerOperation(
            OperationId = "ProbeWeatherByLatLng",
            Summary = "Test thời tiết hiện tại theo tọa độ",
            Description = "Gọi Weather API theo lat/lng và trả snapshot thời tiết + weather risk score/level để phục vụ xác minh Emergency.")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ProbeWeather(
            [FromQuery] double lat,
            [FromQuery] double lng,
            CancellationToken cancellationToken = default)
        {
            var result = await _weatherService.GetCurrentWeatherAsync(lat, lng, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Lấy chi tiết yêu cầu cứu hộ
        /// - GET /api/rescuerequest/{id}
        /// </summary>
        [HttpGet("{id}")]
        [SwaggerOperation(
            OperationId = "GetRescueRequestById",
            Summary = "Lấy chi tiết rescue request",
            Description = "Trả toàn bộ thông tin request: attachments, verifications, operations, weather snapshot và campaign liên quan.")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetRescueRequestById(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _rescueRequestService.GetRescueRequestByIdAsync(id, cancellationToken);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new
                {
                    statusCode = StatusCodes.Status404NotFound,
                    message = ex.Message,
                    traceId = HttpContext.TraceIdentifier
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    statusCode = StatusCodes.Status500InternalServerError,
                    message = "An error occurred",
                    detail = ex.Message,
                    traceId = HttpContext.TraceIdentifier
                });
            }
        }

        [HttpPost("{id}/verify")]
        [Authorize(Roles = "Moderator,Manager,Admin")]
        [SwaggerOperation(
            OperationId = "VerifyRescueRequest",
            Summary = "Xác minh rescue request",
            Description = "Moderator/Manager/Admin duyệt hoặc từ chối request bằng RequestVerification (status/method/reason/note). Dùng cho bước kiểm tra nghiệp vụ trước khi xử lý tiếp.")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> VerifyRescueRequest(
            Guid id,
            [FromBody] VerifyRescueRequestDto request,
            CancellationToken cancellationToken = default)
        {
            var result = await _rescueRequestService.VerifyRescueRequestAsync(id, request, cancellationToken);
            return Ok(result);
        }

        [HttpPost("{id}/assign-team")]
        [Authorize(Roles = "Moderator,Manager,Admin")]
        [SwaggerOperation(
            OperationId = "AssignTeamToRescue",
            Summary = "Gán 1 team cho 1 rescue request",
            Description = "Gán team thuộc trạm đã dispatch cho request. Đồng thời cập nhật operation status và đưa request vào queue batch active của team.")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AssignRescueTeam(
            Guid id,
            [FromBody] AssignRescueTeamRequestDto request,
            CancellationToken cancellationToken = default)
        {
            var result = await _rescueRequestService.AssignTeamToRescueAsync(id, request, cancellationToken);
            return Ok(result);
        }

        [HttpPost("assign-team-bulk")]
        [Authorize(Roles = "Moderator,Manager,Admin")]
        [SwaggerOperation(
            OperationId = "AssignTeamToRescueBulk",
            Summary = "Gán 1 team cho nhiều rescue request trong một lần gọi",
            Description = "Bulk assign theo danh sách requestIds. Kết quả trả về chi tiết từng request thành công/thất bại. Các request hợp lệ sẽ được xếp queue theo RescueBatch/RescueBatchItem.")]
        [ProducesResponseType(typeof(BulkAssignRescueTeamResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AssignRescueTeamBulk(
            [FromBody] AssignRescueTeamBulkRequestDto request,
            CancellationToken cancellationToken = default)
        {
            var result = await _rescueRequestService.AssignTeamToMultipleRescueRequestsAsync(request, cancellationToken);
            return Ok(result);
        }

        [HttpPost("{id}/operations/{operationId}/complete")]
        [Authorize(Roles = "Volunteer")]
        [SwaggerOperation(
            OperationId = "CompleteRescueOperation",
            Summary = "Team leader xác nhận hoàn tất cứu hộ (bắt buộc ảnh hiện trường)",
            Description = "Leader của team được assign operation gửi bằng chứng ảnh + ghi chú để xác nhận đã cứu xong tại hiện trường. Sau đó operation chuyển RescueCompleted, queue item chuyển Done và tự đẩy item kế tiếp nếu có.")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CompleteRescueOperation(
            Guid id,
            Guid operationId,
            [FromBody] CompleteRescueOperationRequestDto request,
            CancellationToken cancellationToken = default)
        {
            var result = await _rescueRequestService.CompleteRescueOperationAsync(id, operationId, request, cancellationToken);
            return Ok(result);
        }

        [HttpGet("teams/{teamId}/active-batch")]
        [SwaggerOperation(
            OperationId = "GetActiveRescueBatchByTeam",
            Summary = "Lấy queue nhiệm vụ active của team",
            Description = "Trả RescueBatch active hiện tại của team, gồm danh sách RescueBatchItem theo SequenceOrder để frontend hiển thị hàng đợi xử lý.")]
        [ProducesResponseType(typeof(RescueBatchQueueResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetActiveBatchByTeam(
            Guid teamId,
            CancellationToken cancellationToken = default)
        {
            var result = await _rescueRequestService.GetActiveBatchByTeamAsync(teamId, cancellationToken);
            if (result == null)
            {
                return NotFound(new
                {
                    statusCode = StatusCodes.Status404NotFound,
                    message = "Active rescue batch not found for this team.",
                    traceId = HttpContext.TraceIdentifier
                });
            }

            return Ok(result);
        }

        [HttpPatch("teams/{teamId}/active-batch/reorder")]
        [Authorize(Roles = "Moderator,Manager,Admin")]
        [SwaggerOperation(
            OperationId = "ReorderActiveRescueBatch",
            Summary = "Đổi thứ tự queue nhiệm vụ của team",
            Description = "Sắp xếp lại thứ tự request trong batch active theo RequestIdsInOrder. Item đầu queue sẽ được set InProgress; các item còn lại Pending (trừ item Done/Cancelled).")]
        [ProducesResponseType(typeof(RescueBatchQueueResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ReorderBatchQueue(
            Guid teamId,
            [FromBody] ReorderRescueBatchRequestDto request,
            CancellationToken cancellationToken = default)
        {
            var result = await _rescueRequestService.ReorderBatchQueueAsync(teamId, request, cancellationToken);
            return Ok(result);
        }

        [HttpPatch("{id}/operations/{operationId}/status")]
        [Authorize(Roles = "Volunteer,Moderator,Manager,Admin")]
        [SwaggerOperation(
            OperationId = "UpdateRescueOperationStatus",
            Summary = "Cập nhật trạng thái operation theo tiến độ cứu hộ",
            Description = "Cho phép cập nhật các trạng thái tác nghiệp: EnRoute, Rescuing, Returning, Closed, Cancelled. Dùng để frontend update timeline hành trình cứu hộ theo thời gian thực.")]
        [ProducesResponseType(typeof(RescueRequestResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateRescueOperationStatus(
            Guid id,
            Guid operationId,
            [FromBody] UpdateRescueOperationStatusRequestDto request,
            CancellationToken cancellationToken = default)
        {
            var result = await _rescueRequestService.UpdateRescueOperationStatusAsync(id, operationId, request, cancellationToken);
            return Ok(result);
        }

    }
}
