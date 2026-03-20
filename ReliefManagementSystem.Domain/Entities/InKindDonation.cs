using ReliefManagementSystem.Domain.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ReliefManagementSystem.Domain.Entities
{
    [Obsolete("Public in-kind donations are deprecated. Use procurement workflow instead.")]
    public class InKindDonation
    {
        [Key]
        public Guid InKindDonationId { get; set; }

        public Guid? CampaignId { get; set; }
        public Campaign? Campaign { get; set; }

        public Guid ReliefStationId { get; set; }
        public ReliefStation ReliefStation { get; set; } = null!;

        public Guid? DonorUserId { get; set; }
        public ApplicationUser? DonorUser { get; set; }

        public bool IsAnonymous { get; set; } = false;

        public string? DonorName { get; set; }

        public string? DonorContact { get; set; }

        public string? Message { get; set; }

        public DateTime DonatedAt { get; set; } = DateTime.UtcNow;

        public DonationStatus Status { get; set; } = DonationStatus.Pending;

        public Guid? InventoryTransactionId { get; set; }
        public InventoryTransaction? InventoryTransaction { get; set; }

        public ICollection<InKindDonationDetail> InKindDonationDetails { get; set; } = new List<InKindDonationDetail>();
    }
}
