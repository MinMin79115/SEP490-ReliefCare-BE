using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Domain.Entities
{
    public class ProcurementOrder
    {
        public Guid ProcurementOrderId { get; set; }
        public Guid CampaignId { get; set; }
        public Campaign Campaign { get; set; } = null!;

        public Guid DestinationInventoryId { get; set; }
        public Inventory DestinationInventory { get; set; } = null!;

        public string OrderCode { get; set; } = null!;
        public ProcurementStatus Status { get; set; } = ProcurementStatus.Draft;

        public decimal TotalEstimatedCost { get; set; }
        public decimal? TotalActualCost { get; set; }

        public string? SupplierName { get; set; }
        public string? SupplierContact { get; set; }
        public string? Notes { get; set; }
        public string? ApprovalNote { get; set; }
        public string? ReceiveNote { get; set; }

        public Guid CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public Guid? ApprovedBy { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public Guid? ReceivedBy { get; set; }
        public DateTime? ReceivedAt { get; set; }

        public Guid? InventoryTransactionId { get; set; }
        public InventoryTransaction? InventoryTransaction { get; set; }

        public ICollection<ProcurementOrderItem> Items { get; set; } = new List<ProcurementOrderItem>();
    }
}
