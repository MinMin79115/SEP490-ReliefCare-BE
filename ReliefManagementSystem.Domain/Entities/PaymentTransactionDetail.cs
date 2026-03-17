using ReliefManagementSystem.Domain.Entities.Common;
using System;
using System.ComponentModel.DataAnnotations;

namespace ReliefManagementSystem.Domain.Entities
{
    public class PaymentTransactionDetail : AuditableEntity
    {
        [Key]
        public Guid PaymentTransactionDetailId { get; set; }

        public Guid PaymentTransactionId { get; set; }
        public PaymentTransaction PaymentTransaction { get; set; } = null!;

        public string? FieldName { get; set; }
        public string? FieldValue { get; set; }
    }
}
