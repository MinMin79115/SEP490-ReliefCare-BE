using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReliefManagementSystem.Application.Features.SupplyItem.DTOs.Request;
using ReliefManagementSystem.Application.Interface;
using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.API.Controllers
{
    /// <summary>
    /// Manages supply item master data (catalogue of goods used in relief operations).
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SupplyItemController : ControllerBase
    {
        private readonly ISupplyItemService _supplyItemService;

        public SupplyItemController(ISupplyItemService supplyItemService)
        {
            _supplyItemService = supplyItemService;
        }

        /// <summary>
        /// Creates a new supply item.
        /// </summary>
        /// <response code="200">Supply item created successfully.</response>
        /// <response code="400">Validation error or duplicate name.</response>
        [HttpPost]
        public async Task<IActionResult> CreateSupplyItem(
            [FromBody] CreateSupplyItemRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                var result = await _supplyItemService.CreateSupplyItemAsync(request, cancellationToken);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Gets paginated supply items, with optional category filter.
        /// </summary>
        /// <param name="category">Optional category to filter by.</param>
        /// <param name="pageIndex">Page number, default 1.</param>
        /// <param name="pageSize">Items per page, default 20.</param>
        /// <response code="200">Paged list of supply items.</response>
        [HttpGet]
        public async Task<IActionResult> GetAllSupplyItems(
            [FromQuery] SupplyCategory? category,
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            var result = await _supplyItemService.GetAllSupplyItemsAsync(category, pageIndex, pageSize, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Gets a supply item by ID.
        /// </summary>
        /// <response code="200">Supply item details.</response>
        /// <response code="404">Supply item not found.</response>
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetSupplyItemById(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _supplyItemService.GetSupplyItemByIdAsync(id, cancellationToken);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Updates an existing supply item.
        /// </summary>
        /// <response code="200">Updated supply item.</response>
        /// <response code="400">Duplicate name.</response>
        /// <response code="404">Supply item not found.</response>
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateSupplyItem(
            Guid id,
            [FromBody] UpdateSupplyItemRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                var result = await _supplyItemService.UpdateSupplyItemAsync(id, request, cancellationToken);
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

        /// <summary>
        /// Deletes a supply item by ID.
        /// </summary>
        /// <response code="200">Supply item deleted successfully.</response>
        /// <response code="404">Supply item not found.</response>
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteSupplyItem(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                await _supplyItemService.DeleteSupplyItemAsync(id, cancellationToken);
                return Ok(new { message = "Supply item deleted successfully." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}
