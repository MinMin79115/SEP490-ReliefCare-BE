using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Domain.Entities
{
    public class CampaignHousehold
    {
        public Guid CampaignHouseholdId { get; set; }

        public Guid CampaignId { get; set; }
        public Guid? DistributionPointId { get; set; }
        public Guid? CampaignTeamId { get; set; }
        public Guid? LocationId { get; set; }

        public string HouseholdCode { get; set; } = string.Empty;
        public string HeadOfHouseholdName { get; set; } = string.Empty;
        public string? ContactPhone { get; set; }
        public string? Address { get; set; }

        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int HouseholdSize { get; set; }
        public bool IsIsolated { get; set; }
        public int? FloodSeverityLevel { get; set; }
        public int? IsolationSeverityLevel { get; set; }
        public bool RequiresBoat { get; set; }
        public bool RequiresLocalGuide { get; set; }
        public DeliveryMode DeliveryMode { get; set; } = DeliveryMode.PickupAtPoint;

        public HouseholdFulfillmentStatus FulfillmentStatus { get; set; } = HouseholdFulfillmentStatus.Pending;
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Campaign Campaign { get; set; } = null!;
        public DistributionPoint? DistributionPoint { get; set; }
        public CampaignTeam? CampaignTeam { get; set; }
        public Location? Location { get; set; }

        public ICollection<HouseholdDelivery> Deliveries { get; set; } = new List<HouseholdDelivery>();
    }
}
