using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Application.Features.Donation.DTOs.Response
{
    public class AdminDonationItemResponse
    {
        public Guid DonationId { get; set; }
        public Guid CampaignId { get; set; }
        public string? CampaignName { get; set; }
        public string DonorName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DonationStatus Status { get; set; }
        public long OrderCode { get; set; }
        public string? PaymentLinkId { get; set; }
        public DateTime DonatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
    }
}
