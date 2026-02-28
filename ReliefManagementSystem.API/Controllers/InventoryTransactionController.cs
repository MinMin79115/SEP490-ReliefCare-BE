using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReliefManagementSystem.Application.Features.InventoryTransaction.DTOs.Request;
using ReliefManagementSystem.Application.Interface;
using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.API.Controllers
{
    /// <summary>
    /// Manages inventory transactions (import/export) and auto-updates stock quantities.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class InventoryTransactionController : ControllerBase
    {
        private readonly IInventoryTransactionService _transactionService;

        public InventoryTransactionController(IInventoryTransactionService transactionService)
        {
            _transactionService = transactionService;
        }

        /// <summary>
        /// Creates a new inventory transaction (import or export).
        /// Automatically updates CurrentQuantity of each affected stock entry.
        /// </summary>
        /// <response code="200">Transaction created, stock updated.</response>
        /// <response code="400">Validation error, insufficient stock, or inactive inventory.</response>
        /// <response code="404">Inventory or supply item not found.</response>
        [HttpPost]
        public async Task<IActionResult> CreateTransaction(
            [FromBody] CreateTransactionRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                var result = await _transactionService.CreateTransactionAsync(request, cancellationToken);
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

        /// <summary>Gets a single transaction with full line-item details.</summary>
        /// <response code="200">Transaction detail.</response>
        /// <response code="404">Transaction not found.</response>
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetTransactionById(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _transactionService.GetTransactionByIdAsync(id, cancellationToken);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        /// <summary>Gets all transactions for a specific inventory, newest first.</summary>
        /// <response code="200">List of transaction summaries.</response>
        /// <response code="404">Inventory not found.</response>
        [HttpGet("by-inventory/{inventoryId:guid}")]
        public async Task<IActionResult> GetByInventory(Guid inventoryId, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _transactionService.GetTransactionsByInventoryAsync(inventoryId, cancellationToken);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Gets transactions filtered by type (Import=1, Export=2).
        /// Optionally filter by inventory.
        /// </summary>
        /// <response code="200">Filtered list of transaction summaries.</response>
        [HttpGet("by-type")]
        public async Task<IActionResult> GetByType(
            [FromQuery] TransactionType type,
            [FromQuery] Guid? inventoryId,
            CancellationToken cancellationToken)
        {
            var result = await _transactionService.GetTransactionsByTypeAsync(type, inventoryId, cancellationToken);
            return Ok(result);
        }
    }
}
