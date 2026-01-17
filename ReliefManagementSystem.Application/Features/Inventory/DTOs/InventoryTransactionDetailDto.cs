using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Application.Features.Inventory.DTOs
{
    public class InventoryTransactionDetailDto
    {
        public Guid TransactionId { get; set; }
        public string TransactionCode { get; set; } = null!;
        public TransactionType Type { get; set; }
        public string TypeName { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public Guid CreatedBy { get; set; }
        public string? CreatedByName { get; set; }
        public string? CreatedByEmail { get; set; }
        public string? Notes { get; set; }
        public List<TransactionItemDetailDto> Items { get; set; } = new();
    }

    public class TransactionItemDetailDto
    {
        public Guid TransactionItemId { get; set; }
        public Guid SupplyItemId { get; set; }
        public string SupplyItemName { get; set; } = null!;
        public string Unit { get; set; } = null!;
        public int Quantity { get; set; }
        public string? Notes { get; set; }
    }
}
