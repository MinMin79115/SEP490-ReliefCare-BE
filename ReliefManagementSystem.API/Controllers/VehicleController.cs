using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReliefManagementSystem.Application.Common.Models;
using ReliefManagementSystem.Application.Features.Vehicle.DTOs.Request;
using ReliefManagementSystem.Application.Features.Vehicle.DTOs.Response;
using ReliefManagementSystem.Application.Interface;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

namespace ReliefManagementSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] 
    public class VehicleController : ControllerBase
    {
        private readonly IVehicleService _vehicleService;

        public VehicleController(IVehicleService vehicleService)
        {
            _vehicleService = vehicleService;
        }

        // POST /api/vehicle
        [HttpPost]
        [SwaggerOperation(
            Summary = "Tạo phương tiện",
            Description = "Role behavior:\n- Manager: Có thể tạo xe với hoặc không gán trạm (ReliefStationId có thể null).\n- Moderator: Khi tạo xe sẽ tự động gán vào trạm của moderator, giá trị ReliefStationId từ request sẽ bị bỏ qua.\n- TeamId chỉ hợp lệ khi team đã được duyệt tại trạm của xe."
        )]
        public async Task<IActionResult> CreateVehicle(
            [FromBody] CreateVehicleRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _vehicleService.CreateVehicleAsync(request, userId, IsManager(), IsModerator(), cancellationToken);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET /api/vehicle
        [HttpGet]
        [SwaggerOperation(
            OperationId = "GetAllVehicles",
            Summary = "Lấy danh sách phương tiện",
            Description = "Role behavior:\n- Manager: Xem toàn bộ phương tiện, có thể lọc theo ReliefStationId.\n- Moderator: Chỉ xem phương tiện thuộc trạm của mình (tự động scope theo trạm, bỏ qua ReliefStationId từ query).\n- Hỗ trợ tìm kiếm theo LicensePlate, TeamName, ReliefStationName, VehicleTypeName và phân trang."
        )]
        public async Task<ActionResult<Pagination<VehicleResponse>>> GetAllVehicles(
            [FromQuery] SearchVehicleRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _vehicleService.GetAllVehiclesAsync(request, userId, IsManager(), IsModerator(), cancellationToken);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET /api/vehicle/counts?stationId=...
        [HttpGet("counts")]
        [Authorize(Roles = "Manager")]
        [SwaggerOperation(
            Summary = "Thống kê phương tiện theo trạng thái",
            Description = "Chỉ Manager được truy cập. Nếu truyền stationId thì thống kê trong 1 trạm; nếu không truyền thì thống kê toàn hệ thống."
        )]
        public async Task<IActionResult> GetVehicleCounts([FromQuery] Guid? stationId, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _vehicleService.GetVehicleCountsAsync(stationId, cancellationToken);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET /api/vehicle/{id}
        [HttpGet("{id:guid}")]
        [SwaggerOperation(
            Summary = "Lấy chi tiết phương tiện",
            Description = "Role behavior:\n- Manager: Xem được phương tiện bất kỳ.\n- Moderator: Chỉ xem được phương tiện thuộc trạm của mình."
        )]
        public async Task<IActionResult> GetVehicleById(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _vehicleService.GetVehicleByIdAsync(id, userId, IsManager(), IsModerator(), cancellationToken);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // GET /api/vehicle/status/{status}
        [HttpGet("status/{status:int}")]
        [SwaggerOperation(
            Summary = "Lấy phương tiện theo trạng thái",
            Description = "Role behavior:\n- Manager: Xem theo trạng thái trên toàn hệ thống.\n- Moderator: Chỉ xem phương tiện theo trạng thái trong trạm của mình.\nStatus: 1 = Free, 2 = Busy."
        )]
        public async Task<IActionResult> GetVehiclesByStatus(int status, CancellationToken cancellationToken)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _vehicleService.GetVehiclesByStatusAsync(status, userId, IsManager(), IsModerator(), cancellationToken);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET /api/vehicle/my-vehicles
        [HttpGet("my-vehicles")]
        [SwaggerOperation(
            Summary = "Lấy phương tiện do user hiện tại tạo",
            Description = "Endpoint này trả về danh sách theo CreatedBy của user hiện tại, không phải scope theo role/trạm."
        )]
        public async Task<IActionResult> GetMyVehicles(CancellationToken cancellationToken)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _vehicleService.GetMyVehiclesAsync(userId, cancellationToken);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET /api/vehicle/available-for-dispatch
        [HttpGet("available-for-dispatch")]
        [Authorize(Roles = "Moderator")]
        [SwaggerOperation(
            OperationId = "GetAvailableVehiclesForDispatch",
            Summary = "Lấy danh sách xe không bận để điều phối cứu hộ",
            Description = "Chỉ Moderator được gọi. Backend tự scope theo trạm hiện tại của moderator và chỉ trả về xe có Status = Free để dùng trong màn hình assign team + vehicle cho rescue request."
        )]
        public async Task<IActionResult> GetAvailableVehiclesForDispatch(CancellationToken cancellationToken)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _vehicleService.GetAvailableVehiclesForModeratorAsync(userId, cancellationToken);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // PUT /api/vehicle/{id}
        [HttpPut("{id:guid}")]
        [SwaggerOperation(
            Summary = "Cập nhật phương tiện",
            Description = "Role behavior:\n- Manager: Có thể cập nhật mọi phương tiện.\n- Moderator: Chỉ cập nhật phương tiện thuộc trạm của mình.\n- User khác role trên: chỉ cập nhật phương tiện do chính mình tạo."
        )]
        public async Task<IActionResult> UpdateVehicle(
            Guid id,
            [FromBody] UpdateVehicleRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _vehicleService.UpdateVehicleAsync(id, request, userId, IsManager(), IsModerator(), cancellationToken);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // DELETE /api/vehicle/{id}
        [HttpDelete("{id:guid}")]
        [SwaggerOperation(
            Summary = "Xóa mềm phương tiện",
            Description = "Role behavior:\n- Manager: Xóa được mọi phương tiện.\n- Moderator: Chỉ xóa phương tiện thuộc trạm của mình.\n- User khác role trên: chỉ xóa phương tiện do chính mình tạo.\nThao tác là soft delete (IsDeleted = true)."
        )]
        public async Task<IActionResult> DeleteVehicle(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var userId = GetCurrentUserId();
                await _vehicleService.DeleteVehicleAsync(id, userId, IsManager(), IsModerator(), cancellationToken);
                return Ok(new { message = "Vehicle deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // PUT /api/vehicle/{id}/assign-station/{stationId}
        [HttpPut("{id:guid}/assign-station/{stationId:guid}")]
        [Authorize(Roles = "Manager")]
        [SwaggerOperation(
            Summary = "Gán phương tiện vào trạm",
            Description = "Chỉ Manager được gọi endpoint này. Dùng cho luồng manager tạo xe trước rồi gán trạm sau. Nếu xe đang có TeamId thì team đó phải được duyệt tại trạm đích."
        )]
        public async Task<IActionResult> AssignVehicleToStation(Guid id, Guid stationId, CancellationToken cancellationToken)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _vehicleService.AssignVehicleToStationAsync(id, stationId, userId, IsManager(), cancellationToken);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // PUT /api/vehicle/{id}/assign-team/{teamId}
        [HttpPut("{id:guid}/assign-team/{teamId:guid}")]
        [Authorize(Roles = "Moderator")]
        [SwaggerOperation(
            Summary = "Gán phương tiện vào team",
            Description = "Chỉ Moderator được gọi. Moderator chỉ gán team cho phương tiện thuộc trạm của mình và team phải được duyệt tại đúng trạm đó."
        )]
        public async Task<IActionResult> AssignVehicleToTeam(Guid id, Guid teamId, CancellationToken cancellationToken)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _vehicleService.AssignVehicleToTeamAsync(id, teamId, userId, cancellationToken);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                throw new UnauthorizedAccessException("User not authenticated");
            }
            return userId;
        }

        private bool IsManager() => User.IsInRole("Manager");

        private bool IsModerator() => User.IsInRole("Moderator");
    }
}
