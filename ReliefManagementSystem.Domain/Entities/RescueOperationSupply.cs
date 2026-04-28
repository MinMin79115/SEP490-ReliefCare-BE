using System;

namespace ReliefManagementSystem.Domain.Entities
{
    public class RescueOperationSupply
    {
        public Guid RescueOperationSupplyId { get; set; }
        public Guid RescueOperationId { get; set; }
        public Guid SourceInventoryId { get; set; }
        public Guid SupplyItemId { get; set; }
        public int Quantity { get; set; }
        public string? Unit { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid? CreatedBy { get; set; }
        public Guid? InventoryTransactionId { get; set; }

        public RescueOperation RescueOperation { get; set; } = default!;
        public Inventory SourceInventory { get; set; } = default!;
        public SupplyItem SupplyItem { get; set; } = default!;
        public InventoryTransaction? InventoryTransaction { get; set; }
    }
}
