namespace ReliefManagementSystem.Application.Common.Exceptions.Donation
{
    public class PayOsWebhookSignatureInvalidException : AppException
    {
        public PayOsWebhookSignatureInvalidException()
            : base("Webhook signature không hợp lệ.", "PAYOS_INVALID_SIGNATURE", 400)
        {
        }
    }
}
