namespace ReliefManagementSystem.Application.Features.Fund.Dtos.Responses
{
    public class FundContributionResponse
    {
        public Guid FundContributionId { get; set; }
        public Guid DonationId { get; set; }
        public Guid CampaignId { get; set; }
        public string CampaignName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime ContributedAt { get; set; }
    }
}
