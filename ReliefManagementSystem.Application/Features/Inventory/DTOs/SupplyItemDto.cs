using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Application.Features.Inventory.DTOs
{
    public class SupplyItemDto
    {
        public Guid SupplyItemId { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public string? IconUrl { get; set; }
        public SupplyCategory Category { get; set; }
        public string CategoryName { get; set; } = null!;
        public string Unit { get; set; } = null!;
        public int CurrentQuantity { get; set; }
        public int MinimumStockLevel { get; set; }
        public int MaximumStockLevel { get; set; }
        public InventoryStatus Status { get; set; }
        public string StatusName { get; set; } = null!;
        public decimal PercentageFull { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
