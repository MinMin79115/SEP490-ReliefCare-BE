namespace ReliefManagementSystem.Application.Common.Exceptions.Donation
{
    public class DonationNotFoundException : AppException
    {
        public DonationNotFoundException(Guid donationId)
            : base($"Không tìm thấy donation với ID: {donationId}", "DONATION_NOT_FOUND", 404)
        {
        }
    }
}
