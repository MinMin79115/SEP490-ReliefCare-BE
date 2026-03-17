using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Application.Features.Donation.DTOs.Response
{
    public class CreateDonationCheckoutResponse
    {
        public Guid DonationId { get; set; }
        public long OrderCode { get; set; }
        public string? PaymentLinkId { get; set; }
        public string CheckoutUrl { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public DonationStatus Status { get; set; }
    }
}
