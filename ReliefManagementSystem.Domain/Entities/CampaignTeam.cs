using ReliefManagementSystem.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Domain.Entities
{
    public class CampaignTeam
    {
        public Guid CampaignTeamId { get; set; }
        public Guid CampaignId { get; set; }
        public Guid TeamId { get; set; }

        public CampaignTeamRole Role { get; set; }
        public CampaignTeamStatus Status { get; set; }

        public DateTime AssignedAt { get; set; }
        public bool IsDelete { get; set; }

        public Campaign Campaign { get; set; } = default!;
        public Team Team { get; set; } = default!;
    }

}
