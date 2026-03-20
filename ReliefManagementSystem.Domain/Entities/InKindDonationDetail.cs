using System;
using System.ComponentModel.DataAnnotations;

namespace ReliefManagementSystem.Domain.Entities
{
    public class InKindDonationDetail
    {
        [Key]
        public Guid InKindDonationDetailId { get; set; }

        public Guid InKindDonationId { get; set; }
        public InKindDonation InKindDonation { get; set; } = null!;

        public Guid SupplyItemId { get; set; }
        public SupplyItem SupplyItem { get; set; } = null!;

        public int Quantity { get; set; }

        public string? Notes { get; set; }
    }
}
