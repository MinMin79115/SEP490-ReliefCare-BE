using System.ComponentModel.DataAnnotations;

namespace ReliefManagementSystem.Application.Features.Inventory.DTOs.Request
{
    /// <summary>
    /// Request model to add a supply item to an inventory (register stock slot).
    /// </summary>
    public class AddStockItemRequest
    {
        /// <summary>The supply item to track in this inventory.</summary>
        [Required(ErrorMessage = "SupplyItemId is required.")]
        public Guid SupplyItemId { get; set; }

        /// <summary>Current quantity on hand.</summary>
        [Range(0, int.MaxValue, ErrorMessage = "CurrentQuantity must be >= 0.")]
        public int CurrentQuantity { get; set; } = 0;

        /// <summary>Threshold below which a restock alert triggers.</summary>
        [Range(0, int.MaxValue, ErrorMessage = "MinimumStockLevel must be >= 0.")]
        public int MinimumStockLevel { get; set; }

        /// <summary>Maximum capacity of the storage slot.</summary>
        [Range(1, int.MaxValue, ErrorMessage = "MaximumStockLevel must be >= 1.")]
        public int MaximumStockLevel { get; set; }
    }
}
