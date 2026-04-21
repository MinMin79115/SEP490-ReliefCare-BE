using System;

namespace ReliefManagementSystem.Domain.Entities
{
    public class CampaignInventoryStock
    {
        public Guid CampaignInventoryStockId { get; set; }
        public Guid CampaignInventoryId { get; set; }
        public Guid SupplyItemId { get; set; }
        public int CurrentQuantity { get; set; }

        public CampaignInventory CampaignInventory { get; set; } = default!;
        public SupplyItem SupplyItem { get; set; } = default!;
    }
}
