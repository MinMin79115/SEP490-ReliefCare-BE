using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReliefManagementSystem.Application.Features.SupplyAllocation.DTOs.Request;
using ReliefManagementSystem.Application.Interface;
using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.API.Controllers
{
    /// <summary>
    /// Manages supply allocation workflow: Pending → Approved → Delivered | Cancelled.
    /// Stock is deducted on Approve and returned on Cancel-after-Approve.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SupplyAllocationController : ControllerBase
    {
        private readonly ISupplyAllocationService _allocationService;

        public SupplyAllocationController(ISupplyAllocationService allocationService)
        {
            _allocationService = allocationService;
        }

        /// <summary>Creates a new supply allocation in Pending status.</summary>
        /// <response code="200">Allocation created.</response>
        /// <response code="400">Validation error (e.g. item not in inventory, duplicates).</response>
        /// <response code="404">Campaign or inventory not found.</response>
        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] CreateSupplyAllocationRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                var result = await _allocationService.CreateAsync(request, cancellationToken);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>Gets a supply allocation by ID with full line-item details.</summary>
        /// <response code="200">Allocation detail.</response>
        /// <response code="404">Allocation not found.</response>
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _allocationService.GetByIdAsync(id, cancellationToken);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        /// <summary>Gets all allocations for a campaign.</summary>
        [HttpGet("by-campaign/{campaignId:guid}")]
        public async Task<IActionResult> GetByCampaign(Guid campaignId, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _allocationService.GetByCampaignAsync(campaignId, cancellationToken);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        /// <summary>Gets all allocations sourced from a specific inventory.</summary>
        [HttpGet("by-inventory/{inventoryId:guid}")]
        public async Task<IActionResult> GetByInventory(Guid inventoryId, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _allocationService.GetByInventoryAsync(inventoryId, cancellationToken);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        /// <summary>Gets allocations filtered by status (0=Pending, 1=Approved, 2=Delivered, 3=Cancelled).</summary>
        [HttpGet("by-status")]
        public async Task<IActionResult> GetByStatus(
            [FromQuery] SupplyAllocationStatus status,
            CancellationToken cancellationToken)
        {
            var result = await _allocationService.GetByStatusAsync(status, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Transitions allocation to a new status.
        /// Valid: Pending→Approved (deducts stock), Pending→Cancelled,
        /// Approved→Delivered, Approved→Cancelled (returns stock).
        /// </summary>
        /// <response code="200">Status updated.</response>
        /// <response code="400">Invalid transition or insufficient stock.</response>
        /// <response code="404">Allocation not found.</response>
        [HttpPatch("{id:guid}/status")]
        public async Task<IActionResult> UpdateStatus(
            Guid id,
            [FromBody] UpdateAllocationStatusRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                var result = await _allocationService.UpdateStatusAsync(id, request, cancellationToken);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
