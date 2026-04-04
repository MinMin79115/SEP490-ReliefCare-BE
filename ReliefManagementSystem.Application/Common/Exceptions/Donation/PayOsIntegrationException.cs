namespace ReliefManagementSystem.Application.Common.Exceptions.Donation
{
    public class PayOsIntegrationException : AppException
    {
        public PayOsIntegrationException(string message)
            : base(message, "PAYOS_INTEGRATION_ERROR", 502)
        {
        }
    }
}
