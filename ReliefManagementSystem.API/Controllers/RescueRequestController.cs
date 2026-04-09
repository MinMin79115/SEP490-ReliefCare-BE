using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<RescueRequestController> _logger;

        public RescueRequestController(
            IRescueRequestService rescueRequestService,
            IWeatherService weatherService,
            ILogger<RescueRequestController> logger)
        {
            _rescueRequestService = rescueRequestService;
            _weatherService = weatherService;
            _logger = logger;
        }

        /// <summary>
        /// Gửi yêu cầu cứu hộ mới
        /// - POST /api/rescuerequest
        /// - Tự động tính priority và dispatch (nếu Emergency type)
        /// - Người dùng không bắt buộc phải đăng nhập (AllowAnonymous)
        /// </summary>
        [HttpPost]
        [AllowAnonymous]
        [EnableRateLimiting("rescue-create")]
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
            [FromQuery] int? verificationStatus,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var result = await _rescueRequestService.SearchRescueRequestsAsync(
                new SearchRescueRequestDto
                {
                    Search = search,
                    StatusFilter = statusFilter,
                    VerificationStatus = verificationStatus,
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

        [HttpPost("{id}/dispatch-preview")]
        [Authorize(Roles = "Moderator,Manager,Admin")]
        [SwaggerOperation(
            OperationId = "PreviewSmartAssignRescue",
            Summary = "Xem trước phương án điều phối thông minh cho request",
            Description = "Dựa trên active batch, vị trí tracking hiện tại của team, priority và loại request để đề xuất queue mới trước khi moderator xác nhận assign.")]
        [ProducesResponseType(typeof(DispatchPreviewResponseDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> PreviewSmartAssign(
            Guid id,
            [FromBody] DispatchPreviewRequestDto request,
            CancellationToken cancellationToken = default)
        {
            var result = await _rescueRequestService.PreviewSmartAssignAsync(id, request, cancellationToken);
            return Ok(result);
        }

        [HttpPost("{id}/smart-assign")]
        [Authorize(Roles = "Moderator,Manager,Admin")]
        [SwaggerOperation(
            OperationId = "SmartAssignRescue",
            Summary = "Điều phối request vào team theo queue thông minh",
            Description = "Assign team cho request và tự động sắp xếp lại active batch theo loại request, priority và độ gần route hiện tại. Emergency có thể chen ngang nếu đủ điều kiện.")]
        [ProducesResponseType(typeof(RescueBatchQueueResponseDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> SmartAssignRescue(
            Guid id,
            [FromBody] SmartAssignRescueTeamRequestDto request,
            CancellationToken cancellationToken = default)
        {
            var result = await _rescueRequestService.SmartAssignTeamToRescueAsync(id, request, cancellationToken);
            return Ok(result);
        }

        [HttpGet("dispatch-candidates")]
        [Authorize(Roles = "Moderator,Manager,Admin")]
        [SwaggerOperation(
            OperationId = "GetDispatchCandidates",
            Summary = "Lấy danh sách request có thể điều phối",
            Description = "Trả danh sách request còn dispatch được theo team. Kèm cờ canDispatch, isInOtherActiveBatch, alreadyAssignedTeamId và lý do block để FE hiển thị đúng UX.")]
        [ProducesResponseType(typeof(PaginatedDispatchCandidatesResponseDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetDispatchCandidates(
            [FromQuery] GetDispatchCandidatesRequestDto request,
            CancellationToken cancellationToken = default)
        {
            var result = await _rescueRequestService.GetDispatchCandidatesAsync(request, cancellationToken);
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
            try
            {
                var result = await _rescueRequestService.CompleteRescueOperationAsync(id, operationId, request, cancellationToken);
                return Ok(result);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                var conflictEntries = ex.Entries.Select(entry => new
                {
                    entity = entry.Metadata.ClrType.Name,
                    state = entry.State.ToString(),
                    keys = entry.Properties
                        .Where(p => p.Metadata.IsPrimaryKey())
                        .ToDictionary(p => p.Metadata.Name, p => p.CurrentValue?.ToString())
                }).ToList();

                _logger.LogError(
                    ex,
                    "CompleteRescueOperation concurrency conflict. RequestId: {RequestId}, OperationId: {OperationId}, TraceId: {TraceId}, Entries: {@Entries}",
                    id,
                    operationId,
                    HttpContext.TraceIdentifier,
                    conflictEntries);

                return Conflict(new
                {
                    statusCode = StatusCodes.Status409Conflict,
                    message = "Nhiệm vụ vừa được cập nhật bởi thao tác khác hoặc dữ liệu đính kèm bị xung đột.",
                    detail = "Vui lòng tải lại dữ liệu nhiệm vụ rồi thử hoàn thành lại một lần nữa.",
                    conflictEntries,
                    requestId = id,
                    operationId,
                    traceId = HttpContext.TraceIdentifier
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "CompleteRescueOperation failed. RequestId: {RequestId}, OperationId: {OperationId}, TraceId: {TraceId}",
                    id,
                    operationId,
                    HttpContext.TraceIdentifier);

                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    statusCode = StatusCodes.Status500InternalServerError,
                    message = "CompleteRescueOperation failed",
                    detail = ex.Message,
                    innerException = ex.InnerException?.Message,
                    requestId = id,
                    operationId,
                    traceId = HttpContext.TraceIdentifier
                });
            }
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

        [HttpPost("teams/{teamId}/active-batch/recalculate-eta")]
        [Authorize(Roles = "Moderator,Manager,Admin")]
        [SwaggerOperation(
            OperationId = "RecalculateActiveBatchEta",
            Summary = "Tính lại ETA queue theo vị trí team mới nhất",
            Description = "Lấy TeamTrackingPoint mới nhất của team, gọi Goong matrix tới các request Pending/InProgress trong batch active và cập nhật DistanceKm/EstimatedMinutes cho từng item.")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> RecalculateActiveBatchEta(
            Guid teamId,
            CancellationToken cancellationToken = default)
        {
            await _rescueRequestService.RecalculateActiveBatchEtaAsync(teamId, cancellationToken);
            return NoContent();
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


        // ─── Extended Endpoints ────────────────────────────────────────────────────

        /// <summary>
        /// Lay danh sach yeu cau cuu ho cua nguoi dung dang dang nhap
        /// - GET /api/rescuerequest/my-requests
        /// </summary>
        [HttpGet("my-requests")]
        [Authorize]
        [SwaggerOperation(
            OperationId = "GetMyRescueRequests",
            Summary = "Lay lich su yeu cau cuu ho cua toi",
            Description = "Nguoi dung da dang nhap xem danh sach cac yeu cau cuu ho ho da gui (phan trang). Co the loc theo statusFilter (int ma enum RescueRequestStatus). Dung cho man hinh 'Lich su yeu cau' tren mobile app.")]
        [ProducesResponseType(typeof(PaginatedRescueRequestResponseDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMyRescueRequests(
            [FromQuery] MyRescueRequestQueryDto query,
            CancellationToken cancellationToken = default)
        {
            var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value!);
            var result = await _rescueRequestService.GetMyRequestsAsync(userId, query, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Nguoi dan tu huy yeu cau cuu ho da gui
        /// - PATCH /api/rescuerequest/{id}/cancel
        /// </summary>
        [HttpPatch("{id}/cancel")]
        [Authorize]
        [SwaggerOperation(
            OperationId = "CancelMyRescueRequest",
            Summary = "Nguoi dan tu huy yeu cau cuu ho",
            Description = "Cho phep chinh chu yeu cau huy khi request con o trang thai Pending (chua duoc gan team). Can cung cap ly do huy. Sau khi huy, mot RequestVerification ghi lu ly do se duoc tao.")]
        [ProducesResponseType(typeof(RescueRequestResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CancelMyRescueRequest(
            Guid id,
            [FromBody] CancelRescueRequestDto request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value!);
                var result = await _rescueRequestService.CancelRescueRequestAsync(id, userId, request, cancellationToken);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { statusCode = 401, message = ex.Message, traceId = HttpContext.TraceIdentifier });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { statusCode = 400, message = ex.Message, traceId = HttpContext.TraceIdentifier });
            }
        }

        /// <summary>
        /// Lay vi tri realtime cua doi cuu ho dang xu ly yeu cau
        /// - GET /api/rescuerequest/{id}/team-location
        /// </summary>
        [HttpGet("{id}/team-location")]
        [AllowAnonymous]
        [SwaggerOperation(
            OperationId = "GetTeamLocationForRequest",
            Summary = "Xem vi tri realtime cua doi cuu ho (khong can dang nhap)",
            Description = "Nguoi dan truy cap bang RequestId de xem to do moi nhat cua doi cuu ho dang tren duong den. Tra ve null neu chua co team nao duoc gan hoac team chua bat dau di chuyen. Khong bao gom thong tin ca nhan cua thanh vien team.")]
        [ProducesResponseType(typeof(TeamLocationForRequestDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetTeamLocationForRequest(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _rescueRequestService.GetTeamLocationForRequestAsync(id, cancellationToken);
                if (result == null)
                    return NotFound(new { statusCode = 404, message = "Chua co doi cuu ho nao duoc gan cho yeu cau nay hoac doi chua bat dau di chuyen.", traceId = HttpContext.TraceIdentifier });
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { statusCode = 404, message = ex.Message, traceId = HttpContext.TraceIdentifier });
            }
        }

        /// <summary>
        /// Dashboard thong ke so luong rescue request theo tung trang thai
        /// - GET /api/rescuerequest/stats
        /// </summary>
        [HttpGet("stats")]
        [Authorize(Roles = "Moderator,Manager,Admin")]
        [SwaggerOperation(
            OperationId = "GetRescueRequestStats",
            Summary = "Thong ke tong hop rescue request theo trang thai",
            Description = "Tra ve so luong tong va so luong chi tiet theo tung trang thai (Pending, Verified, Assigned, InProgress, Completed, Cancelled). Dung de ve bieu do dashboard cho Moderator/Admin.")]
        [ProducesResponseType(typeof(RescueRequestStatsDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetRescueRequestStats(CancellationToken cancellationToken = default)
        {
            var result = await _rescueRequestService.GetRescueStatsAsync(cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Lay lich su cac ca cuu ho da hoan thanh cua mot team
        /// - GET /api/rescuerequest/teams/{teamId}/history
        /// </summary>
        [HttpGet("teams/{teamId}/history")]
        [Authorize]
        [SwaggerOperation(
            OperationId = "GetTeamRescueHistory",
            Summary = "Lich su ca cuu ho (batch) da hoan thanh cua team",
            Description = "Lay danh sach cac ca truc (RescueBatch) da ket thuc cua team, sap xep moi nhat truoc, co phan trang. Moi batch bao gom danh sach cac rescue request da xu ly trong ca do. Dung cho man hinh 'Lich su ca truc' cua tinh nguyen vien va moderator.")]
        [ProducesResponseType(typeof(RescueTeamHistoryResponseDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetTeamRescueHistory(
            Guid teamId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var result = await _rescueRequestService.GetTeamRescueHistoryAsync(teamId, pageNumber, pageSize, cancellationToken);
            return Ok(result);
        }
    }
}

