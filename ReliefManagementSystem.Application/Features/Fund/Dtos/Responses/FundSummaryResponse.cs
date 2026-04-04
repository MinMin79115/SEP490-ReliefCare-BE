namespace ReliefManagementSystem.Application.Features.Fund.Dtos.Responses
{
    public class FundSummaryResponse
    {
        public Guid FundId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal TotalBalance { get; set; }
        public int TotalContributionCount { get; set; }
        public int TotalSourceCampaigns { get; set; }
        public List<FundContributionSourceResponse> Sources { get; set; } = new();
    }

    public class FundContributionSourceResponse
    {
        public Guid CampaignId { get; set; }
        public string CampaignName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal Percentage { get; set; }
    }
}
