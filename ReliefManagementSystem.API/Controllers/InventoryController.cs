using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReliefManagementSystem.Application.Features.Inventory.DTOs;
using ReliefManagementSystem.Application.Interface;
using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InventoryController : ControllerBase
    {
        private readonly IInventoryService _inventoryService;

        public InventoryController(IInventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }

        /// <summary>
        /// Get all supply items with optional filters
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetSupplyItems(
            [FromQuery] SupplyCategory? category,
            [FromQuery] InventoryStatus? status,
            [FromQuery] string? search,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            var result = await _inventoryService.GetSupplyItemsAsync(
                category, status, search, page, pageSize, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Get a supply item by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetSupplyItemById(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var result = await _inventoryService.GetSupplyItemByIdAsync(id, cancellationToken);

            if (result == null)
                return NotFound(new { message = $"Supply item with ID {id} not found" });

            return Ok(result);
        }

        /// <summary>
        /// Create a new supply item
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> CreateSupplyItem(
            [FromBody] CreateSupplyItemRequest request,
            CancellationToken cancellationToken = default)
        {
            var result = await _inventoryService.CreateSupplyItemAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetSupplyItemById), new { id = result.SupplyItemId }, result);
        }

        /// <summary>
        /// Update an existing supply item
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> UpdateSupplyItem(
            Guid id,
            [FromBody] UpdateSupplyItemRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _inventoryService.UpdateSupplyItemAsync(id, request, cancellationToken);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Delete a supply item
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteSupplyItem(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var result = await _inventoryService.DeleteSupplyItemAsync(id, cancellationToken);

            if (!result)
                return NotFound(new { message = $"Supply item with ID {id} not found" });

            return NoContent();
        }

        /// <summary>
        /// Bulk import items into inventory
        /// </summary>
        [HttpPost("bulk-import")]
        [Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> BulkImport(
            [FromBody] BulkImportRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _inventoryService.BulkImportAsync(request, cancellationToken);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Bulk export items from inventory
        /// </summary>
        [HttpPost("bulk-export")]
        [Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> BulkExport(
            [FromBody] BulkExportRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _inventoryService.BulkExportAsync(request, cancellationToken);
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
