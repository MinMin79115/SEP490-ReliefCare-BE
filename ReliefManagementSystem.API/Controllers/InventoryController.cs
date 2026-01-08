using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReliefManagementSystem.Application.Features.Inventory;
using ReliefManagementSystem.Application.Services;
using System.Security.Claims;

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
        /// Get dashboard statistics
        /// </summary>
        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard(CancellationToken cancellationToken)
        {
            var stats = await _inventoryService.GetDashboardStatsAsync(cancellationToken);
            return Ok(stats);
        }

        /// <summary>
        /// Get all inventory items, optionally filtered by category
        /// </summary>
        [HttpGet("items")]
        public async Task<IActionResult> GetAllItems(
            [FromQuery] Guid? categoryId,
            CancellationToken cancellationToken)
        {
            var items = await _inventoryService.GetAllItemsAsync(categoryId, cancellationToken);
            return Ok(items);
        }

        /// <summary>
        /// Get inventory item by ID
        /// </summary>
        [HttpGet("items/{id}")]
        public async Task<IActionResult> GetItemById(
            Guid id,
            CancellationToken cancellationToken)
        {
            var item = await _inventoryService.GetItemByIdAsync(id, cancellationToken);
            if (item == null)
                return NotFound(new { message = $"Item with ID {id} not found" });

            return Ok(item);
        }

        /// <summary>
        /// Create new inventory item
        /// </summary>
        [HttpPost("items")]
        public async Task<IActionResult> CreateItem(
            [FromBody] CreateInventoryItemRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                var item = await _inventoryService.CreateItemAsync(request, cancellationToken);
                return CreatedAtAction(nameof(GetItemById), new { id = item.InventoryItemId }, item);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Update inventory item
        /// </summary>
        [HttpPut("items/{id}")]
        public async Task<IActionResult> UpdateItem(
            Guid id,
            [FromBody] UpdateInventoryItemRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                var item = await _inventoryService.UpdateItemAsync(id, request, cancellationToken);
                return Ok(item);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Delete inventory item
        /// </summary>
        [HttpDelete("items/{id}")]
        public async Task<IActionResult> DeleteItem(
            Guid id,
            CancellationToken cancellationToken)
        {
            try
            {
                await _inventoryService.DeleteItemAsync(id, cancellationToken);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Bulk import items to warehouse
        /// </summary>
        [HttpPost("batch-import")]
        public async Task<IActionResult> BulkImport(
            [FromBody] BulkImportRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _inventoryService.BulkImportAsync(request, userId, cancellationToken);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Bulk export items from warehouse
        /// </summary>
        [HttpPost("batch-export")]
        public async Task<IActionResult> BulkExport(
            [FromBody] BulkExportRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _inventoryService.BulkExportAsync(request, userId, cancellationToken);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get all batches (import/export history)
        /// </summary>
        [HttpGet("batches")]
        public async Task<IActionResult> GetBatches(
            [FromQuery] Domain.Enum.TransactionType? type,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            var batches = await _inventoryService.GetBatchesAsync(type, page, pageSize, cancellationToken);
            return Ok(batches);
        }

        /// <summary>
        /// Get batch detail by ID
        /// </summary>
        [HttpGet("batches/{id}")]
        public async Task<IActionResult> GetBatchDetail(
            Guid id,
            CancellationToken cancellationToken)
        {
            var batch = await _inventoryService.GetBatchDetailAsync(id, cancellationToken);
            if (batch == null)
                return NotFound(new { message = $"Batch with ID {id} not found" });

            return Ok(batch);
        }

        /// <summary>
        /// Get all categories
        /// </summary>
        [HttpGet("categories")]
        public async Task<IActionResult> GetCategories(CancellationToken cancellationToken)
        {
            var categories = await _inventoryService.GetCategoriesAsync(cancellationToken);
            return Ok(categories);
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                // For testing without authentication, return a default GUID
                // In production, this should throw an exception
                return Guid.Parse("00000000-0000-0000-0000-000000000001");
            }
            return userId;
        }
    }
}
