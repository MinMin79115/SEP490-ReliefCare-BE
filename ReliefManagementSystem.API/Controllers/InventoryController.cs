using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReliefManagementSystem.Application.Features.Inventory.DTOs.Request;
using ReliefManagementSystem.Application.Interface;

namespace ReliefManagementSystem.API.Controllers
{
    /// <summary>
    /// Manages inventories and their stock items for relief stations.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class InventoryController : ControllerBase
    {
        private readonly IInventoryService _inventoryService;

        public InventoryController(IInventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }

        // ─── Inventory Endpoints ──────────────────────────────────────────────

        /// <summary>Creates a new inventory for a relief station.</summary>
        /// <response code="200">Inventory created successfully.</response>
        /// <response code="400">Duplicate level for the same station.</response>
        [HttpPost]
        public async Task<IActionResult> CreateInventory(
            [FromBody] CreateInventoryRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                var result = await _inventoryService.CreateInventoryAsync(request, cancellationToken);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>Gets paginated inventories, optionally filtered by relief station and level.</summary>
        /// <param name="reliefStationId">Optional filter by station ID.</param>
        /// <param name="level">Optional filter by inventory level.</param>
        /// <param name="pageIndex">Page number, default 1.</param>
        /// <param name="pageSize">Items per page, default 10.</param>
        [HttpGet]
        public async Task<IActionResult> GetAllInventories(
            [FromQuery] Guid? reliefStationId,
            [FromQuery] ReliefManagementSystem.Domain.Enum.InventoryLevel? level,
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var result = await _inventoryService.GetAllInventoriesAsync(
                reliefStationId, level, pageIndex, pageSize, cancellationToken);
            return Ok(result);
        }

        /// <summary>Gets a single inventory with full stock details.</summary>
        /// <response code="200">Inventory detail.</response>
        /// <response code="404">Inventory not found.</response>
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetInventoryById(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _inventoryService.GetInventoryByIdAsync(id, cancellationToken);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        /// <summary>Updates level and status of an inventory.</summary>
        /// <response code="200">Updated inventory.</response>
        /// <response code="400">Duplicate level conflict.</response>
        /// <response code="404">Inventory not found.</response>
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateInventory(
            Guid id,
            [FromBody] UpdateInventoryRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                var result = await _inventoryService.UpdateInventoryAsync(id, request, cancellationToken);
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

        /// <summary>Soft-deletes an inventory (sets status to Deleted).</summary>
        /// <response code="200">Inventory deleted.</response>
        /// <response code="404">Inventory not found.</response>
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteInventory(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                await _inventoryService.DeleteInventoryAsync(id, cancellationToken);
                return Ok(new { message = "Inventory deleted successfully." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // ─── Stock Endpoints (nested under /api/inventory/{id}/stocks) ────────

        /// <summary>Gets paginated stock entries for a specific inventory.</summary>
        /// <param name="id">Inventory ID.</param>
        /// <param name="pageIndex">Page number, default 1.</param>
        /// <param name="pageSize">Items per page, default 20.</param>
        /// <response code="200">Paged list of stock entries.</response>
        /// <response code="404">Inventory not found.</response>
        [HttpGet("{id:guid}/stocks")]
        public async Task<IActionResult> GetStocks(
            Guid id,
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _inventoryService.GetStocksByInventoryIdAsync(id, pageIndex, pageSize, cancellationToken);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        /// <summary>Adds a supply item slot to an inventory.</summary>
        /// <response code="200">Stock entry created.</response>
        /// <response code="400">Duplicate supply item or inactive inventory.</response>
        /// <response code="404">Inventory or supply item not found.</response>
        [HttpPost("{id:guid}/stocks")]
        public async Task<IActionResult> AddStockItem(
            Guid id,
            [FromBody] AddStockItemRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                var result = await _inventoryService.AddStockItemAsync(id, request, cancellationToken);
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

        /// <summary>Updates Min/Max thresholds for a stock entry.</summary>
        /// <response code="200">Updated stock entry.</response>
        /// <response code="400">Invalid threshold values.</response>
        /// <response code="404">Stock entry not found.</response>
        [HttpPut("stocks/{stockId:guid}")]
        public async Task<IActionResult> UpdateStockItem(
            Guid stockId,
            [FromBody] UpdateStockItemRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                var result = await _inventoryService.UpdateStockItemAsync(stockId, request, cancellationToken);
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

        /// <summary>Removes a supply item slot from an inventory.</summary>
        /// <response code="200">Stock entry removed.</response>
        /// <response code="404">Stock entry not found.</response>
        [HttpDelete("stocks/{stockId:guid}")]
        public async Task<IActionResult> RemoveStockItem(Guid stockId, CancellationToken cancellationToken)
        {
            try
            {
                await _inventoryService.RemoveStockItemAsync(stockId, cancellationToken);
                return Ok(new { message = "Stock item removed successfully." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}
