using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReliefManagementSystem.Application.Features.Vehicle.DTOs.Request;
using ReliefManagementSystem.Application.Interface;
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
        public async Task<IActionResult> CreateVehicle(
            [FromBody] CreateVehicleRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _vehicleService.CreateVehicleAsync(request, userId, cancellationToken);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET /api/vehicle
        [HttpGet]
        public async Task<IActionResult> GetAllVehicles(CancellationToken cancellationToken)
        {
            try
            {
                var result = await _vehicleService.GetAllVehiclesAsync(cancellationToken);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET /api/vehicle/{id}
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetVehicleById(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _vehicleService.GetVehicleByIdAsync(id, cancellationToken);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // GET /api/vehicle/status/{status}
        [HttpGet("status/{status:int}")]
        public async Task<IActionResult> GetVehiclesByStatus(int status, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _vehicleService.GetVehiclesByStatusAsync(status, cancellationToken);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET /api/vehicle/my-vehicles
        [HttpGet("my-vehicles")]
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

        // PUT /api/vehicle/{id}
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateVehicle(
            Guid id,
            [FromBody] UpdateVehicleRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _vehicleService.UpdateVehicleAsync(id, request, userId, cancellationToken);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // DELETE /api/vehicle/{id}
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteVehicle(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _vehicleService.DeleteVehicleAsync(id, userId, cancellationToken);
                return Ok(new { message = "Vehicle deleted successfully" });
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
    }
}