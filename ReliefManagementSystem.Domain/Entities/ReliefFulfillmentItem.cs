using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Domain.Entities
{
    public class ReliefFulfillmentItem
    {
        public Guid ReliefFulfillmentItemId { get; set; }
        public Guid ReliefFulfillmentId { get; set; }
        public Guid SupplyItemId { get; set; }
        public ReliefNeedType? NeedCategory { get; set; }
        public decimal PlannedQuantity { get; set; }
        public decimal ActualDeliveredQuantity { get; set; }
        public string? Note { get; set; }

        public ReliefFulfillment ReliefFulfillment { get; set; } = default!;
        public SupplyItem SupplyItem { get; set; } = default!;
    }
}
