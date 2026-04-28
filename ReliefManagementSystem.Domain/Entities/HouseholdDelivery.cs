using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Domain.Entities
{
    public class HouseholdDelivery
    {
        public Guid HouseholdDeliveryId { get; set; }

        public Guid CampaignId { get; set; }
        public Guid CampaignHouseholdId { get; set; }
        public Guid? DistributionPointId { get; set; }
        public Guid? CampaignTeamId { get; set; }
        public Guid ReliefPackageDefinitionId { get; set; }
        public Guid? DeliveredByUserId { get; set; }

        public DeliveryMode DeliveryMode { get; set; }
        public HouseholdFulfillmentStatus Status { get; set; } = HouseholdFulfillmentStatus.Pending;

        public DateTime ScheduledAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public decimal CashSupportAmount { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Campaign Campaign { get; set; } = null!;
        public CampaignHousehold CampaignHousehold { get; set; } = null!;
        public DistributionPoint? DistributionPoint { get; set; }
        public CampaignTeam? CampaignTeam { get; set; }
        public ReliefPackageDefinition ReliefPackageDefinition { get; set; } = null!;
        public ApplicationUser? DeliveredByUser { get; set; }

        public ICollection<HouseholdDeliveryProof> Proofs { get; set; } = new List<HouseholdDeliveryProof>();
        public ICollection<MemberTaskDelivery> MemberTaskDeliveries { get; set; } = new List<MemberTaskDelivery>();
    }
}
