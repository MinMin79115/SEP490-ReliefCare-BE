using System.ComponentModel.DataAnnotations;

namespace ReliefManagementSystem.Application.Features.Donation.DTOs.Request
{
    public class CreateDonationCheckoutRequest
    {
        [Required]
        public Guid CampaignId { get; set; }

        [Range(1000, 1000000000)]
        public decimal Amount { get; set; }

        [Required]
        [MaxLength(255)]
        public string DonorName { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Message { get; set; }

        [MaxLength(1000)]
        public string? ReturnUrl { get; set; }

        [MaxLength(1000)]
        public string? CancelUrl { get; set; }
    }
}
