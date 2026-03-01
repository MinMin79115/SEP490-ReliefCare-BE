using System.ComponentModel.DataAnnotations;

namespace ReliefManagementSystem.Application.Features.Inventory.DTOs.Request
{
    /// <summary>
    /// Request model to update stock levels for an existing inventory stock entry.
    /// </summary>
    public class UpdateStockItemRequest
    {
        /// <summary>New minimum stock level.</summary>
        [Range(0, int.MaxValue, ErrorMessage = "MinimumStockLevel must be >= 0.")]
        public int MinimumStockLevel { get; set; }

        /// <summary>New maximum stock level.</summary>
        [Range(1, int.MaxValue, ErrorMessage = "MaximumStockLevel must be >= 1.")]
        public int MaximumStockLevel { get; set; }
    }
}
