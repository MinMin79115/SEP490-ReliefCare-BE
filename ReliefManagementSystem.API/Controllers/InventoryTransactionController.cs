using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReliefManagementSystem.Application.Interface;
using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.API.Controllers
{
    [Route("api/inventory-transactions")]
    [ApiController]
    [Authorize]
    public class InventoryTransactionController : ControllerBase
    {
        private readonly IInventoryService _inventoryService;

        public InventoryTransactionController(IInventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }

        /// <summary>
        /// Get all inventory transactions with optional filters
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetTransactions(
            [FromQuery] TransactionType? type,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            var result = await _inventoryService.GetTransactionsAsync(
                type, startDate, endDate, page, pageSize, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Get a transaction by ID with all items
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTransactionById(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var result = await _inventoryService.GetTransactionByIdAsync(id, cancellationToken);

            if (result == null)
                return NotFound(new { message = $"Transaction with ID {id} not found" });

            return Ok(result);
        }
    }
}
