namespace ReliefManagementSystem.Domain.Entities
{
    public class InventoryTransactionItem
    {
        public Guid TransactionItemId { get; set; }

        public Guid TransactionId { get; set; }
        public InventoryTransaction Transaction { get; set; } = null!;

        public Guid SupplyItemId { get; set; }
        public SupplyItem SupplyItem { get; set; } = null!;

        public int Quantity { get; set; }

        public string? Notes { get; set; } // Notes riêng cho từng item
    }
}
