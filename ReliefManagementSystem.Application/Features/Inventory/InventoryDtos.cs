using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Application.Features.Inventory
{
    public class CategoryDto
    {
        public Guid CategoryId { get; set; }
        public string Code { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
    }

    public class InventoryItemDto
    {
        public Guid InventoryItemId { get; set; }
        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; } = null!;
        public string Code { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public string Unit { get; set; } = null!;
        public decimal CurrentQuantity { get; set; }
        public decimal MaxCapacity { get; set; }
        public decimal MinThreshold { get; set; }
        public InventoryStatus Status { get; set; }
        public string StatusText { get; set; } = null!;
        public decimal PercentageFilled { get; set; }
    }

    public class CreateInventoryItemRequest
    {
        public Guid CategoryId { get; set; }
        public string Code { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public string Unit { get; set; } = null!;
        public decimal InitialQuantity { get; set; }
        public decimal MaxCapacity { get; set; }
    }

    public class UpdateInventoryItemRequest
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public decimal MaxCapacity { get; set; }
    }
}
