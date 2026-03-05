using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReliefManagementSystem.Application.Common.Models;
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

        // ──────────────────────────────────────────────────────────────
        //  GET api/relief-stations
        // ──────────────────────────────────────────────────────────────

        /// <summary>
        /// Lấy danh sách tất cả trạm cứu trợ có phân trang.
        /// </summary>
        /// <remarks>
        /// **Hướng dẫn sử dụng:**
        /// - Gọi không có filter → trả về tất cả trạm (mọi cấp).
        /// - Thêm `level` để lọc: 1 = Regional, 2 = Provincial, 3 = Local.
        /// - Thêm `search` để tìm kiếm theo tên trạm (chứa chuỗi — contains).
        ///
        /// **Ví dụ URL:**
        /// - Tất cả: `GET /api/relief-stations?pageIndex=1&amp;pageSize=10`
        /// - Chỉ trạm vùng: `GET /api/relief-stations?level=1`
        /// - Chỉ trạm tỉnh: `GET /api/relief-stations?level=2`
        /// - Chỉ trạm địa phương: `GET /api/relief-stations?level=3`
        /// - Tìm kiếm: `GET /api/relief-stations?search=Bình Dương`
        /// - Kết hợp: `GET /api/relief-stations?level=2&amp;search=Bình&amp;pageIndex=1&amp;pageSize=5`
        /// </remarks>
        /// <param name="request">Query string params: pageIndex, pageSize, level, search.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>Danh sách trạm phân trang kèm thông tin TotalCount, TotalPages, HasNext, HasPrevious.</returns>
        [HttpGet]
        [SwaggerOperation(
            Summary = "Danh Sách Trạm Cứu Trợ",
            Description = "Lấy danh sách tất cả trạm cứu trợ có phân trang. " +
                          "Hỗ trợ lọc theo cấp trạm (level): 1=Regional, 2=Provincial, 3=Local. " +
                          "Hỗ trợ tìm kiếm theo tên trạm (search)."
        )]
        [ProducesResponseType(typeof(Pagination<ReliefStationResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllStations(
            [FromQuery] GetAllStationsRequest request,
            CancellationToken ct)
        {
            var result = await _stationService.GetAllStationsAsync(request, ct);
            return Ok(result);
        }

        // ──────────────────────────────────────────────────────────────
        //  POST api/relief-stations/provincial
        // ──────────────────────────────────────────────────────────────

        /// <summary>
        /// [Manager] Tạo trạm cứu trợ cấp Tỉnh (Provincial).
        /// </summary>
        /// <remarks>
        /// **Quy tắc nghiệp vụ:**
        /// - Chỉ user có role **Manager** mới được gọi API này.
        /// - `locationId` phải là ID của một địa điểm **cấp Tỉnh** (level = 2).
        ///   Gọi `GET /api/locations?level=2` để lấy danh sách.
        /// - Hệ thống **tự động** tìm và gán `parentReliefStationId` = trạm Regional
        ///   của vùng mà Manager đang phụ trách.
        ///
        /// **Ví dụ request body:**
        /// ```json
        /// {
        ///   "name": "Trạm Bình Dương",
        ///   "locationId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        ///   "address": "123 Đại lộ Bình Dương",
        ///   "contactNumber": "0271000000",
        ///   "longitude": 106.6297,
        ///   "latitude": 11.0686
        /// }
        /// ```
        ///
        /// **Các lỗi có thể xảy ra:**
        /// | Status | ErrorCode | Mô tả |
        /// |--------|-----------|-------|
        /// | 400 | INVALID_LOCATION_LEVEL | `locationId` không phải cấp Tỉnh |
        /// | 403 | UNAUTHORIZED_STATION_CREATION | User không có ManagerProfile |
        /// | 404 | LOCATION_NOT_FOUND | `locationId` không tồn tại |
        /// | 404 | PARENT_STATION_NOT_FOUND | Không có trạm Regional cha trong vùng Manager phụ trách |
        /// </remarks>
        /// <param name="request">Thông tin trạm tỉnh cần tạo.</param>
        /// <param name="ct">Cancellation token (tự động inject bởi framework).</param>
        /// <returns>Thông tin trạm vừa được tạo, bao gồm <c>parentReliefStationId</c> được gán tự động.</returns>
        [HttpPost("provincial")]
        [Authorize(Roles = "Manager,Admin")]
        [SwaggerOperation(
            Summary = "Tạo Trạm Cấp Tỉnh",
            Description = "Tạo Trạm Cấp Tỉnh, chỉ cho phép Manager Và Admin được tạo, " +
                          "Nếu Manager tạo trạm thì chỉ có thể tạo được các trạm tính thuộc vùng mình quản lý, " +
                          "ví dụ Manager quản lý trạm miền Nam thì chỉ cho phép tạo trạm ở khu vực tỉnh miền Nam "
        )]
        [ProducesResponseType(typeof(ReliefStationResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CreateProvincialStation(
            [FromBody] CreateProvincialStationRequest request,
            CancellationToken ct)
        {
            var result = await _stationService.CreateProvincialStationAsync(request, ct);
            return CreatedAtAction(nameof(CreateProvincialStation), new { id = result.ReliefStationId }, result);
        }

        // ──────────────────────────────────────────────────────────────
        //  POST api/relief-stations/local
        // ──────────────────────────────────────────────────────────────

        /// <summary>
        /// [Moderator – trưởng trạm tỉnh] Tạo trạm cứu trợ cấp Địa phương (Local).
        /// </summary>
        /// <remarks>
        /// **Quy tắc nghiệp vụ:**
        /// - Chỉ user có role **Moderator** và **IsStationHead = true** tại một trạm **Provincial** mới được gọi API này.
        /// - `locationId` phải là ID của một địa điểm **cấp Xã/Phường** (level = 3).
        ///   Gọi `GET /api/locations?level=3` để lấy danh sách.
        /// - Hệ thống **tự động** gán `parentReliefStationId` = trạm Provincial
        ///   mà Moderator đang đứng đầu.
        ///
        /// **Ví dụ request body:**
        /// ```json
        /// {
        ///   "name": "Trạm Phường An Phú",
        ///   "locationId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        ///   "address": "456 Đường XYZ, An Phú",
        ///   "contactNumber": "0274000000",
        ///   "longitude": 106.73,
        ///   "latitude": 10.80
        /// }
        /// ```
        ///
        /// **Các lỗi có thể xảy ra:**
        /// | Status | ErrorCode | Mô tả |
        /// |--------|-----------|-------|
        /// | 400 | INVALID_LOCATION_LEVEL | `locationId` không phải cấp Xã/Phường |
        /// | 403 | UNAUTHORIZED_STATION_CREATION | User không phải Moderator trưởng trạm |
        /// | 404 | LOCATION_NOT_FOUND | `locationId` không tồn tại |
        /// | 404 | PARENT_STATION_NOT_FOUND | Moderator chưa được gán vào trạm Provincial |
        /// </remarks>
        /// <param name="request">Thông tin trạm địa phương cần tạo.</param>
        /// <param name="ct">Cancellation token (tự động inject bởi framework).</param>
        /// <returns>Thông tin trạm vừa được tạo, bao gồm <c>parentReliefStationId</c> được gán tự động.</returns>
        [HttpPost("local")]
        [Authorize(Roles = "Moderator,Admin")]
        [SwaggerOperation(
            Summary = "Tạo Trạm Cấp Địa Phương",
            Description = "Tạo Trạm Cấp Địa Phương (Local), chỉ cho phép Moderator trưởng trạm tỉnh và Admin được tạo. " +
                          "Moderator phải là người đứng đầu (IsStationHead) của một trạm Provincial. " +
                          "Hệ thống tự động gán parentReliefStationId = trạm tỉnh mà Moderator đang đứng đầu."
        )]
        [ProducesResponseType(typeof(ReliefStationResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CreateLocalStation(
            [FromBody] CreateLocalStationRequest request,
            CancellationToken ct)
        {
            var result = await _stationService.CreateLocalStationAsync(request, ct);
            return CreatedAtAction(nameof(CreateLocalStation), new { id = result.ReliefStationId }, result);
        }
    }
}
