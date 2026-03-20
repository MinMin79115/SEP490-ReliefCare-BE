using ReliefManagementSystem.Domain.Entities.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ReliefManagementSystem.Domain.Entities
{
    public class PaymentTransaction : AuditableEntity
    {
        [Key]
        public Guid PaymentTransactionId { get; set; }

        public Guid? DonationId { get; set; }
        public Donation? Donation { get; set; }

        public Guid? UserId { get; set; }
        public ApplicationUser? User { get; set; }

        public string Provider { get; set; } = "PayOS";
        public long OrderCode { get; set; }
        public string? PaymentLinkId { get; set; }
        public string? Reference { get; set; }
        public string? EventCode { get; set; }
        public string? EventDescription { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "VND";
        public DateTime? TransactionDateTime { get; set; }
        public string? CounterAccountName { get; set; }
        public string? CounterAccountNumber { get; set; }
        public string? CounterAccountBankName { get; set; }
        public string? VirtualAccountName { get; set; }
        public string? VirtualAccountNumber { get; set; }
        public string RawPayload { get; set; } = string.Empty;
        public string Signature { get; set; } = string.Empty;
        public bool IsSignatureValid { get; set; }
    }
}
