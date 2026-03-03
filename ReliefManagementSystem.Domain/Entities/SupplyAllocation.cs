using ReliefManagementSystem.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Domain.Entities
{
    public class SupplyAllocation
    {
        public Guid AllocationId { get; set; }

        public Guid CampaignId { get; set; }
        public Guid SourceInventoryId { get; set; }

        public DateTime AllocatedAt { get; set; } = DateTime.UtcNow;
        public SupplyAllocationStatus Status { get; set; }

        // Navigation
        public Campaign Campaign { get; set; } = default!;
        public Inventory SourceInventory { get; set; } = default!;
        public ICollection<SupplyAllocationItem> Items { get; set; } = new List<SupplyAllocationItem>();
    }
}
