namespace ReliefManagementSystem.Domain.Entities
{
    public class CampaignBudgetTransfer
    {
        public Guid CampaignBudgetTransferId { get; set; }
        public Guid SourceCampaignId { get; set; }
        public Guid TargetCampaignId { get; set; }
        public decimal Amount { get; set; }
        public Guid? TransferredByUserId { get; set; }
        public DateTime TransferredAt { get; set; } = DateTime.UtcNow;
        public string? Note { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? CancelledAt { get; set; }
        public Guid? CancelledByUserId { get; set; }

        public Campaign SourceCampaign { get; set; } = null!;
        public Campaign TargetCampaign { get; set; } = null!;
        public ApplicationUser? TransferredByUser { get; set; }
        public ApplicationUser? CancelledByUser { get; set; }
    }
}
