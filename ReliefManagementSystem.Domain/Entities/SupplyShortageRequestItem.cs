namespace ReliefManagementSystem.Domain.Entities
{
    public class SupplyShortageRequestItem
    {
        public Guid SupplyShortageRequestItemId { get; set; }

        public Guid SupplyShortageRequestId { get; set; }
        public Guid SupplyItemId { get; set; }
        public int QuantityRequested { get; set; }
        public int? QuantityApproved { get; set; }
        public string? Note { get; set; }

        public SupplyShortageRequest SupplyShortageRequest { get; set; } = null!;
        public SupplyItem SupplyItem { get; set; } = null!;
    }
}
