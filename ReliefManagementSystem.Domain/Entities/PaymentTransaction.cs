using ReliefManagementSystem.Domain.Entities.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ReliefManagementSystem.Domain.Entities
{
    /// <summary>
    /// Lưu trữ thông tin response từ webhook của payment gateway (ví dụ SePay) liên quan đến Donation.
    /// </summary>
    public class PaymentTransaction : AuditableEntity
    {
        [Key]
        public Guid PaymentTransactionId { get; set; }

        public Guid? DonationId { get; set; }
        public Donation? Donation { get; set; }

        public Guid? UserId { get; set; }
        public ApplicationUser? User { get; set; }

        // Webhook original response data
        public string? GatewayId { get; set; } // id từ payload (e.g. "1")
        public string? GatewayCustomerId { get; set; } // customer_id
        public string? OrderId { get; set; } // order_id (e.g. "SEPAY-68B01673A77FF")
        public string? OrderInvoiceNumber { get; set; } // order_invoice_number
        public string? OrderStatus { get; set; } // order_status (e.g. "CAPTURED")
        public decimal OrderAmount { get; set; } // order_amount
        public string? OrderCurrency { get; set; } // order_currency
        public string? OrderDescription { get; set; } // order_description
        public string? AuthenticationStatus { get; set; } // authentication_status

        // Transaction timestamps từ payload
        public DateTime? PayloadCreatedAt { get; set; } // created_at
        public DateTime? PayloadUpdatedAt { get; set; } // updated_at

        // Danh sách các giao dịch con
        public ICollection<PaymentTransactionDetail> TransactionDetails { get; set; } = new List<PaymentTransactionDetail>();
    }
}
