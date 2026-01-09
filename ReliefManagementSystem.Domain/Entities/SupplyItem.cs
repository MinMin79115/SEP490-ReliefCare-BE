using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Domain.Entities
{
    public class SupplyItem
    {
        public Guid SupplyItemId { get; set; }

        public string Name { get; set; } = null!;

        public string? Description { get; set; }

        public SupplyCategory Category { get; set; }

        public string Unit { get; set; } = null!; // "Thùng", "Cái", "Hộp", "Bộ"

        public int CurrentQuantity { get; set; }

        public int MinimumStockLevel { get; set; }

        public int MaximumStockLevel { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Computed property for status
        public InventoryStatus Status
        {
            get
            {
                if (MaximumStockLevel == 0) return InventoryStatus.Critical;

                var percentage = (decimal)CurrentQuantity / MaximumStockLevel * 100;

                if (percentage >= 100) return InventoryStatus.Full;
                if (percentage >= 50) return InventoryStatus.Safe;
                if (percentage >= 15) return InventoryStatus.NeedRestock;
                return InventoryStatus.Critical;
            }
        }

        // Navigation properties
        public ICollection<InventoryTransactionItem> TransactionItems { get; set; } = new List<InventoryTransactionItem>();
    }
}
