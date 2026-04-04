using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Domain.Entities
{
    public class SupplyAllocationItem
    {
        public Guid AllocationItemId { get; set; }

        public Guid AllocationId { get; set; }
        public Guid SupplyItemId { get; set; }

        public int Quantity { get; set; }

        // Navigation
        public SupplyAllocation SupplyAllocation { get; set; } = default!;
        public SupplyItem SupplyItem { get; set; } = default!;

        public ICollection<CampaignTaskItem> CampaignTaskItems { get; set; } = new List<CampaignTaskItem>();
        public ICollection<DistributionSessionItem> DistributionSessionItems { get; set; } = new List<DistributionSessionItem>();
    }
}
