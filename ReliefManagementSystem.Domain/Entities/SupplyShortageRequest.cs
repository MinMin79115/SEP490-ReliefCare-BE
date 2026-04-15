using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Domain.Entities
{
    public class SupplyShortageRequest
    {
        public Guid SupplyShortageRequestId { get; set; }

        public Guid CampaignId { get; set; }
        public Guid? DistributionPointId { get; set; }
        public Guid? CampaignTeamId { get; set; }
        public Guid RequestedByUserId { get; set; }

        public SupplyShortageRequestStatus Status { get; set; } = SupplyShortageRequestStatus.Pending;
        public string? Reason { get; set; }
        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ReviewedAt { get; set; }
        public Guid? ReviewedByUserId { get; set; }
        public string? ReviewNote { get; set; }

        public Campaign Campaign { get; set; } = null!;
        public DistributionPoint? DistributionPoint { get; set; }
        public CampaignTeam? CampaignTeam { get; set; }
        public ApplicationUser RequestedByUser { get; set; } = null!;
        public ApplicationUser? ReviewedByUser { get; set; }

        public ICollection<SupplyShortageRequestItem> Items { get; set; } = new List<SupplyShortageRequestItem>();
    }
}
