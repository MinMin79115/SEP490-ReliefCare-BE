using System;
using System.Collections.Generic;

namespace ReliefManagementSystem.Domain.Entities
{
    public class CampaignInventory
    {
        public Guid CampaignInventoryId { get; set; }
        public Guid CampaignId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Campaign Campaign { get; set; } = default!;
        public ICollection<CampaignInventoryStock> Stocks { get; set; } = new List<CampaignInventoryStock>();
        public ICollection<CampaignInventoryTransaction> Transactions { get; set; } = new List<CampaignInventoryTransaction>();
    }
}
