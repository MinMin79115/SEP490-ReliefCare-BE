using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Application.Features.Procurement.Dtos.Responses
{
    public class ProcurementOrderResponse
    {
        public Guid ProcurementOrderId { get; set; }
        public Guid CampaignId { get; set; }
        public Guid DestinationInventoryId { get; set; }
        public string OrderCode { get; set; } = string.Empty;
        public ProcurementStatus Status { get; set; }
        public decimal TotalEstimatedCost { get; set; }
        public decimal? TotalActualCost { get; set; }
        public string? SupplierName { get; set; }
        public string? SupplierContact { get; set; }
        public string? Notes { get; set; }
        public string? ApprovalNote { get; set; }
        public string? ReceiveNote { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid? ApprovedBy { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public Guid? ReceivedBy { get; set; }
        public DateTime? ReceivedAt { get; set; }
        public Guid? InventoryTransactionId { get; set; }
        public List<ProcurementOrderItemResponse> Items { get; set; } = new();
    }

    public class ProcurementOrderItemResponse
    {
        public Guid ProcurementOrderItemId { get; set; }
        public Guid SupplyItemId { get; set; }
        public string SupplyItemName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitCost { get; set; }
        public int? ReceivedQuantity { get; set; }
        public decimal? ActualUnitCost { get; set; }
    }
}
