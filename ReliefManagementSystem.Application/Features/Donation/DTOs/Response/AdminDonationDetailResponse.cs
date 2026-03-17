using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Application.Features.Donation.DTOs.Response
{
    public class AdminDonationDetailResponse
    {
        public Guid DonationId { get; set; }
        public Guid CampaignId { get; set; }
        public string? CampaignName { get; set; }
        public string DonorName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string? Message { get; set; }
        public DonationStatus Status { get; set; }
        public long OrderCode { get; set; }
        public string? PaymentLinkId { get; set; }
        public string? CheckoutUrl { get; set; }
        public DateTime DonatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public string? GatewayResponse { get; set; }
        public List<AdminPaymentTransactionResponse> Transactions { get; set; } = new();
    }

    public class AdminPaymentTransactionResponse
    {
        public Guid PaymentTransactionId { get; set; }
        public string Provider { get; set; } = string.Empty;
        public string? Reference { get; set; }
        public string? EventCode { get; set; }
        public string? EventDescription { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public bool IsSignatureValid { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
