using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Application.Features.Inventory.DTOs
{
    public class InventoryTransactionDto
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
        public int ItemCount { get; set; }
        public int TotalQuantity { get; set; }
    }
}
