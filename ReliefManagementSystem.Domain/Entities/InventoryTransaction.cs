using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Domain.Entities
{
    public class InventoryTransaction
    {
        public Guid TransactionId { get; set; }

        public string TransactionCode { get; set; } = null!; // "IN-20260109-001", "OUT-20260109-002"

        public TransactionType Type { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Guid CreatedBy { get; set; }
        public ApplicationUser CreatedByUser { get; set; } = null!;

        public string? Notes { get; set; }

        // Navigation properties - One transaction can have multiple items
        public ICollection<InventoryTransactionItem> Items { get; set; } = new List<InventoryTransactionItem>();
    }
}
