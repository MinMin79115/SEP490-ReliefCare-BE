using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Domain.Entities
{
    public class DistributionPoint
    {
        public Guid DistributionPointId { get; set; }

        public Guid CampaignId { get; set; }
        public Guid ReliefStationId { get; set; }
        public Guid? CampaignTeamId { get; set; }
        public Guid? LocationId { get; set; }

        public string Name { get; set; } = string.Empty;
        public string? Address { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DeliveryMode DeliveryMode { get; set; } = DeliveryMode.PickupAtPoint;

        public DateTime StartsAt { get; set; }
        public DateTime? EndsAt { get; set; }
        public bool IsActive { get; set; } = true;

        public Campaign Campaign { get; set; } = null!;
        public ReliefStation ReliefStation { get; set; } = null!;
        public CampaignTeam? CampaignTeam { get; set; }
        public Location? Location { get; set; }

        public ICollection<CampaignHousehold> Households { get; set; } = new List<CampaignHousehold>();
        public ICollection<HouseholdDelivery> Deliveries { get; set; } = new List<HouseholdDelivery>();
    }
}
