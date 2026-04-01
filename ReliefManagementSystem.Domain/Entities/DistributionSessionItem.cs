namespace ReliefManagementSystem.Domain.Entities
{
    public class DistributionSessionItem
    {
        public Guid DistributionSessionItemId { get; set; }
        public Guid DistributionSessionId { get; set; }
        public Guid SupplyItemId { get; set; }
        public Guid? SupplyAllocationItemId { get; set; }
        public decimal ReservedQuantity { get; set; }
        public decimal DeliveredQuantity { get; set; }

        public DistributionSession DistributionSession { get; set; } = default!;
        public SupplyItem SupplyItem { get; set; } = default!;
        public SupplyAllocationItem? SupplyAllocationItem { get; set; }
    }
}
