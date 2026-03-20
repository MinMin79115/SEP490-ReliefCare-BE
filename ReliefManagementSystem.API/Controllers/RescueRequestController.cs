using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ReliefManagementSystem.Application.Features.RescueRequest.DTOs.Request;
using ReliefManagementSystem.Application.Interface;
using ReliefManagementSystem.Application.Services;
using System;
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

        public RescueRequestController(IRescueRequestService rescueRequestService)
        {
            _rescueRequestService = rescueRequestService;
        }

        /// <summary>
        /// Gửi yêu cầu cứu hộ mới
        /// - POST /api/rescuerequest
        /// - Tự động tính priority và dispatch (nếu Emergency type)
        /// - Người dùng không bắt buộc phải đăng nhập (AllowAnonymous)
        /// </summary>
        [HttpPost]
        [AllowAnonymous]
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
        /// Lấy chi tiết yêu cầu cứu hộ
        /// - GET /api/rescuerequest/{id}
        /// </summary>
        [HttpGet("{id}")]
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

        /// <summary>
        /// Lấy danh sách yêu cầu cứu hộ (có phân trang)
        /// - GET /api/rescuerequest?pageNumber=1&pageSize=10&statusFilter=0
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetRescueRequests(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] int? statusFilter = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _rescueRequestService.GetRescueRequestsAsync(
                    pageNumber,
                    pageSize,
                    statusFilter,
                    cancellationToken);
                return Ok(result);
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

        /// <summary>
        /// Xác minh yêu cầu cứu hộ (Admin/Manager only)
        /// - POST /api/rescuerequest/{id}/verify
        /// - Kết quả: 0 = Approved, 1 = Rejected
        /// - Yêu cầu: User phải có role Admin hoặc Manager
        /// </summary>
        [HttpPost("{id}/verify")]
        [Authorize(Roles = "Admin,Manager")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> VerifyRescueRequest(
            Guid id,
            [FromBody] VerifyRescueRequestDto request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Get current user ID from claims
                var userIdClaim = User.FindFirst("sub")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var verifierId))
                    return Unauthorized(new
                    {
                        statusCode = StatusCodes.Status401Unauthorized,
                        message = "Unable to determine current user",
                        traceId = HttpContext.TraceIdentifier
                    });

                var result = await _rescueRequestService.VerifyRescueRequestAsync(id, request, cancellationToken);
                return Ok(result);
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
                    message = "An error occurred while verifying the request",
                    detail = ex.Message,
                    traceId = HttpContext.TraceIdentifier
                });
            }
        }

        /// <summary>
        /// Lấy danh sách yêu cầu cứu hộ chờ xác minh
        /// - GET /api/rescuerequest/pending/list
        /// - Yêu cầu: User phải có role Admin hoặc Manager
        /// </summary>
        [HttpGet("pending/list")]
        [Authorize(Roles = "Admin,Manager")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetPendingRescueRequests(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _rescueRequestService.GetRescueRequestsAsync(
                    pageNumber,
                    pageSize,
                    (int)Domain.Enum.RescueRequestStatus.Pending,
                    cancellationToken);
                return Ok(result);
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
    }
}