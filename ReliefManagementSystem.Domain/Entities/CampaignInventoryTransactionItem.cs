using System;

namespace ReliefManagementSystem.Domain.Entities
{
    public class CampaignInventoryTransactionItem
    {
        public Guid CampaignInventoryTransactionItemId { get; set; }
        public Guid CampaignInventoryTransactionId { get; set; }
        public Guid SupplyItemId { get; set; }
        public int Quantity { get; set; }
        public string? Notes { get; set; }

        public CampaignInventoryTransaction CampaignInventoryTransaction { get; set; } = default!;
        public SupplyItem SupplyItem { get; set; } = default!;
    }
}
