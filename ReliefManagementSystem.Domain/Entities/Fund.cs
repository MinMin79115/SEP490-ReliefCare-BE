namespace ReliefManagementSystem.Domain.Entities
{
    public class Fund
    {
        public Guid FundId { get; set; }
        public string Name { get; set; } = null!;
        public decimal TotalBalance { get; set; }
        public bool IsDefault { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<FundContribution> Contributions { get; set; } = new List<FundContribution>();
        public ICollection<FundTransaction> Transactions { get; set; } = new List<FundTransaction>();
    }
}
