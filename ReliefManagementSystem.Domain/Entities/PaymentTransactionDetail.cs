using ReliefManagementSystem.Domain.Entities.Common;
using System;
using System.ComponentModel.DataAnnotations;

namespace ReliefManagementSystem.Domain.Entities
{
    /// <summary>
    /// Lưu trữ chi tiết từng giao dịch trong mảng "transactions" của webhook payload.
    /// </summary>
    public class PaymentTransactionDetail : AuditableEntity
    {
        [Key]
        public Guid PaymentTransactionDetailId { get; set; }

        public Guid PaymentTransactionId { get; set; }
        public PaymentTransaction PaymentTransaction { get; set; } = null!;

        public string? GatewayTransactionId { get; set; } // id từ item (e.g. "1")
        public string? PaymentMethod { get; set; } // payment_method (e.g. "CARD")
        public string? TransactionType { get; set; } // transaction_type
        public decimal TransactionAmount { get; set; } // transaction_amount
        public string? TransactionCurrency { get; set; } // transaction_currency
        public string? TransactionStatus { get; set; } // transaction_status (e.g. "APPROVED")
        public string? AuthenticationStatus { get; set; } // authentication_status
        public string? CardNumber { get; set; } // card_number
        public string? CardHolderName { get; set; } // card_holder_name
        public string? CardExpiry { get; set; } // card_expiry
        public string? CardFundingMethod { get; set; } // card_funding_method
        public string? CardBrand { get; set; } // card_brand
        public DateTime? TransactionDate { get; set; } // transaction_date
        public DateTime? TransactionLastUpdatedDate { get; set; } // transaction_last_updated_date
    }
}
