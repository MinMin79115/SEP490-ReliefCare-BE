using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Application.Features.Donation.DTOs.Request
{
    public class AdminDonationQueryRequest
    {
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public DonationStatus? Status { get; set; }
        public Guid? CampaignId { get; set; }
        public string? Keyword { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}
