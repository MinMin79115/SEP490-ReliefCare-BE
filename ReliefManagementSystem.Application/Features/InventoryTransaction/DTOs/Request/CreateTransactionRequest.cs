using ReliefManagementSystem.Domain.Enum;
using System.ComponentModel.DataAnnotations;

namespace ReliefManagementSystem.Application.Features.InventoryTransaction.DTOs.Request
{
    /// <summary>
    /// Request model to create a new inventory transaction (import or export).
    /// Header + line items submitted together for atomicity.
    /// </summary>
    public class CreateTransactionRequest
    {
        /// <summary>The inventory this transaction belongs to.</summary>
        [Required(ErrorMessage = "InventoryId is required.")]
        public Guid InventoryId { get; set; }

        /// <summary>Import (1 - nhập kho) or Export (2 - xuất kho).</summary>
        [Required(ErrorMessage = "Type is required.")]
        public TransactionType Type { get; set; }

        /// <summary>
        /// Reason for this transaction:
        /// Donation (1), SupplyTransferIn (2), SupplyTransferOut (3),
        /// CampaignAllocation (4), Other (5).
        /// </summary>
        [Required(ErrorMessage = "Reason is required.")]
        public TransactionReason Reason { get; set; }

        /// <summary>Optional note for the overall transaction.</summary>
        [MaxLength(500)]
        public string? Notes { get; set; }

        /// <summary>Line items — at least one required.</summary>
        [Required]
        [MinLength(1, ErrorMessage = "At least one item is required.")]
        public List<TransactionItemRequest> Items { get; set; } = [];
    }

    /// <summary>One line item within a transaction.</summary>
    public class TransactionItemRequest
    {
        /// <summary>The supply item being imported or exported.</summary>
        [Required(ErrorMessage = "SupplyItemId is required.")]
        public Guid SupplyItemId { get; set; }

        /// <summary>Quantity to move. Must be >= 1.</summary>
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; set; }

        /// <summary>Optional per-item note.</summary>
        [MaxLength(200)]
        public string? Notes { get; set; }
    }
}
