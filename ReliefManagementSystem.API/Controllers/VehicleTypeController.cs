using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReliefManagementSystem.Application.Features.VehicleType.DTOs.Request;
using ReliefManagementSystem.Application.Interface;

namespace ReliefManagementSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] 
    public class VehicleTypeController : ControllerBase
    {
        private readonly IVehicleTypeService _vehicleTypeService;

        public VehicleTypeController(IVehicleTypeService vehicleTypeService)
        {
            _vehicleTypeService = vehicleTypeService;
        }

        // POST /api/vehicletype
        [HttpPost]
        public async Task<IActionResult> CreateVehicleType(
            [FromBody] CreateVehicleTypeRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                var result = await _vehicleTypeService.CreateVehicleTypeAsync(request, cancellationToken);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET /api/vehicletype
        [HttpGet]
        public async Task<IActionResult> GetAllVehicleTypes(CancellationToken cancellationToken)
        {
            try
            {
                var result = await _vehicleTypeService.GetAllVehicleTypesAsync(cancellationToken);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET /api/vehicletype/{id}
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetVehicleTypeById(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _vehicleTypeService.GetVehicleTypeByIdAsync(id, cancellationToken);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // PUT /api/vehicletype/{id}
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateVehicleType(
            Guid id,
            [FromBody] UpdateVehicleTypeRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                var result = await _vehicleTypeService.UpdateVehicleTypeAsync(id, request, cancellationToken);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // DELETE /api/vehicletype/{id}
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteVehicleType(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _vehicleTypeService.DeleteVehicleTypeAsync(id, cancellationToken);
                return Ok(new { message = "Vehicle Type deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}