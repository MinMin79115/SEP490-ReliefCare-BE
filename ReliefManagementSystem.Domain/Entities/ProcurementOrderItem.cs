namespace ReliefManagementSystem.Domain.Entities
{
    public class ProcurementOrderItem
    {
        public Guid ProcurementOrderItemId { get; set; }
        public Guid ProcurementOrderId { get; set; }
        public ProcurementOrder ProcurementOrder { get; set; } = null!;

        public Guid SupplyItemId { get; set; }
        public SupplyItem SupplyItem { get; set; } = null!;

        public int Quantity { get; set; }
        public decimal UnitCost { get; set; }
        public int? ReceivedQuantity { get; set; }
        public decimal? ActualUnitCost { get; set; }
    }
}
