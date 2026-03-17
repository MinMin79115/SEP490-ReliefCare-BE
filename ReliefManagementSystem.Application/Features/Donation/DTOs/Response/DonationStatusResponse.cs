using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Application.Features.Donation.DTOs.Response
{
    public class DonationStatusResponse
    {
        public Guid DonationId { get; set; }
        public long OrderCode { get; set; }
        public decimal Amount { get; set; }
        public string DonorName { get; set; } = string.Empty;
        public DonationStatus Status { get; set; }
        public DateTime DonatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public string? CheckoutUrl { get; set; }
    }
}
