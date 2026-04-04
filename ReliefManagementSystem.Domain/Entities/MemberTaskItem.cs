using ReliefManagementSystem.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Domain.Entities
{
    public class MemberTaskItem
    {
        public Guid MemberTaskItemId { get; set; }

        public Guid MemberTaskId { get; set; }
        public Guid CampaignTaskItemId { get; set; }

        public int QuantityAssigned { get; set; }
        public int QuantityDelivered { get; set; }

        // Navigation
        public MemberTask MemberTask { get; set; } = default!;
        public CampaignTaskItem CampaignTaskItem { get; set; } = default!;
    }
}
