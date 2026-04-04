using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Application.Features.Fund.Dtos.Responses
{
    public class FundTransactionResponse
    {
        public Guid FundTransactionId { get; set; }
        public FundTransactionType Type { get; set; }
        public decimal Amount { get; set; }
        public decimal BalanceAfter { get; set; }
        public string? Description { get; set; }
        public Guid? CampaignId { get; set; }
        public string? CampaignName { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
