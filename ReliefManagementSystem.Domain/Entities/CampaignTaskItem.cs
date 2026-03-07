using ReliefManagementSystem.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Domain.Entities
{
    public class CampaignTaskItem
    {
        public Guid CampaignTaskItemId { get; set; }

        public Guid CampaignTaskId { get; set; }
        public Guid SupplyAllocationItemId { get; set; }

        public int QuantityAssigned { get; set; }
        public int QuantityDelivered { get; set; }

        // Navigation
        public CampaignTask CampaignTask { get; set; } = default!;
        public SupplyAllocationItem SupplyAllocationItem { get; set; } = default!;
        
        public ICollection<MemberTaskItem> MemberTaskItems { get; set; } = new List<MemberTaskItem>();
    }
}
