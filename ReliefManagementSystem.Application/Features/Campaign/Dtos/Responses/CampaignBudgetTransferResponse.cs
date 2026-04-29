namespace ReliefManagementSystem.Application.Features.Campaign.Dtos.Responses
{
    public class CampaignBudgetTransferResponse
    {
        public Guid CampaignBudgetTransferId { get; set; }
        public Guid SourceCampaignId { get; set; }
        public Guid TargetCampaignId { get; set; }
        public decimal Amount { get; set; }
        public Guid? TransferredByUserId { get; set; }
        public string? TransferredByUserName { get; set; }
        public DateTime TransferredAt { get; set; }
        public string? Note { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? CancelledAt { get; set; }
        public Guid? CancelledByUserId { get; set; }
        public string? CancelledByUserName { get; set; }
        public decimal SourceRemainingBudget { get; set; }
        public decimal TargetRemainingBudget { get; set; }
    }
}
