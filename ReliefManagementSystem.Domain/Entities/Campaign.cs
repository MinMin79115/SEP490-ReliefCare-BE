using ReliefManagementSystem.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Domain.Entities
{
    public class Campaign
    {
        public Guid CampaignId { get; set; }

        public Guid LocationId { get; set; }

        public Guid CreatedByStationId { get; set; }

        public Guid CreatedBy { get; set; }  

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public CampaignStatus Status { get; set; }

        public Location Location { get; set; } = default!;
        public ReliefStation CreatedByStation { get; set; } = default!;

        public ICollection<CampaignTeam> CampaignTeams { get; set; } = new List<CampaignTeam>();
        public ICollection<CampaignTask> CampaignTasks { get; set; } = new List<CampaignTask>();
    }
}
