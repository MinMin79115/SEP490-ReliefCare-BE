using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Application.Features.Campaign.Dtos.Responses
{
    public class CampaignSummaryResponse
    {
        public Guid CampaignId { get; set; }
        public string Name { get; set; } = string.Empty;
        public CampaignStatus Status { get; set; }
        public CampaignType Type { get; set; }
        public CampaignCompletionRule CompletionRule { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool AllowOverTarget { get; set; }
        public decimal OverallProgressPercent { get; set; }
        public decimal BudgetTotal { get; set; }
        public decimal BudgetSpent { get; set; }
        public decimal RemainingBudget { get; set; }
    }
}
