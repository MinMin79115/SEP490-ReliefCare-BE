namespace ReliefManagementSystem.Application.Common.Exceptions.Donation
{
    public class DonationInvalidStateException : AppException
    {
        public DonationInvalidStateException(string message)
            : base(message, "DONATION_INVALID_STATE", 400)
        {
        }
    }
}
