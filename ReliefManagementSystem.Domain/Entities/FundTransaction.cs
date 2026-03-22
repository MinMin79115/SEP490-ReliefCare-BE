using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Domain.Entities
{
    public class FundTransaction
    {
        public Guid FundTransactionId { get; set; }

        public Guid FundId { get; set; }
        public Fund Fund { get; set; } = null!;

        public FundTransactionType Type { get; set; }
        public decimal Amount { get; set; }
        public decimal BalanceAfter { get; set; }

        public Guid? FundContributionId { get; set; }
        public FundContribution? FundContribution { get; set; }

        public string? Description { get; set; }
        public Guid? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
