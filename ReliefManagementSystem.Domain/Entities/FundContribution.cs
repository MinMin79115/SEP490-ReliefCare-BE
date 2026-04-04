namespace ReliefManagementSystem.Domain.Entities
{
    public class FundContribution
    {
        public Guid FundContributionId { get; set; }

        public Guid FundId { get; set; }
        public Fund Fund { get; set; } = null!;

        public Guid DonationId { get; set; }
        public Donation Donation { get; set; } = null!;

        public Guid CampaignId { get; set; }
        public Campaign Campaign { get; set; } = null!;

        public decimal Amount { get; set; }
        public DateTime ContributedAt { get; set; } = DateTime.UtcNow;
    }
}
