namespace ReliefManagementSystem.Application.Common.Exceptions.Donation
{
    public class DonationCampaignNotFoundException : AppException
    {
        public DonationCampaignNotFoundException(Guid campaignId)
            : base($"Không tìm thấy campaign với ID: {campaignId}", "DONATION_CAMPAIGN_NOT_FOUND", 404)
        {
        }
    }
}
