using ReliefManagementSystem.Application.Features.Donation.DTOs.Request;

namespace ReliefManagementSystem.Application.Interface
{
    public interface IPayOsGateway
    {
        Task<PayOsCreatePaymentResult> CreatePaymentLinkAsync(
            long orderCode,
            int amount,
            string description,
            string buyerName,
            string? buyerEmail,
            string? buyerPhone,
            string? returnUrl,
            string? cancelUrl,
            DateTime expiresAtUtc,
            CancellationToken cancellationToken = default);

        Task<PayOsPaymentInfoResult> GetPaymentLinkInfoAsync(string idOrOrderCode, CancellationToken cancellationToken = default);

        Task<PayOsPaymentInfoResult> CancelPaymentLinkAsync(string idOrOrderCode, string? reason, CancellationToken cancellationToken = default);

        bool VerifyWebhook(PayOsWebhookRequest request);
    }

    public class PayOsCreatePaymentResult
    {
        public long OrderCode { get; set; }
        public int Amount { get; set; }
        public string? PaymentLinkId { get; set; }
        public string CheckoutUrl { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }

    public class PayOsPaymentInfoResult
    {
        public string? PaymentLinkId { get; set; }
        public long OrderCode { get; set; }
        public int Amount { get; set; }
        public int AmountPaid { get; set; }
        public int AmountRemaining { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
