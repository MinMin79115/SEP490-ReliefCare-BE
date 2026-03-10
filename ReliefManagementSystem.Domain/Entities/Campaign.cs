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

        public Guid CreatedBy { get; set; }
        public string Name { get; set; }

        public string? Description { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public double AreaRadiusKm { get; set; }
        public string? AddressDetail { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public CampaignStatus Status { get; set; }

        public CampaignType Type { get; set; }
        public decimal BudgetTotal { get; set; }
        public decimal BudgetSpent { get; set; }
        public Location Location { get; set; } = default!;

        public ICollection<CampaignTeam> CampaignTeams { get; set; } = new List<CampaignTeam>();
        public ICollection<CampaignTask> CampaignTasks { get; set; } = new List<CampaignTask>();
        public ICollection<ReliefRequest> ReliefRequests { get; set; } = new List<ReliefRequest>();
        public ICollection<Donation> Donations { get; set; } = new List<Donation>();
        public ICollection<InKindDonation> InKindDonations { get; set; } = new List<InKindDonation>();
        
        public ICollection<CampaignStation> CampaignStations { get; set; } = new List<CampaignStation>();
    }
}
