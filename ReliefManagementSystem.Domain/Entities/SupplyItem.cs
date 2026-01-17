using ReliefManagementSystem.Domain.Enum;
using System.ComponentModel.DataAnnotations;

namespace ReliefManagementSystem.Domain.Entities
{
    public class SupplyItem
    {
        public Guid SupplyItemId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = null!;

        [MaxLength(1000)]
        public string? Description { get; set; }

        [MaxLength(500)]
        public string? IconUrl { get; set; }

        [Required]
        public SupplyCategory Category { get; set; }

        [Required]
        [MaxLength(50)]
        public string Unit { get; set; } = null!; // "Thùng", "Cái", "Hộp", "Bộ"

        [Range(0, int.MaxValue, ErrorMessage = "Current quantity cannot be negative")]
        public int CurrentQuantity { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Minimum stock level must be greater than or equal to 0")]
        public int MinimumStockLevel { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Maximum stock level must be greater than 0")]
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
