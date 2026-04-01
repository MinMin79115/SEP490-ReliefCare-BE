using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Domain.Entities
{
    public class ReliefFulfillment
    {
        public Guid ReliefFulfillmentId { get; set; }
        public Guid ReliefRequestId { get; set; }
        public Guid DistributionSessionId { get; set; }
        public int WaveNumber { get; set; }
        public DistributionSessionMode Mode { get; set; }
        public ReliefFulfillmentStatus Status { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public string? RecipientName { get; set; }
        public string? RecipientPhone { get; set; }
        public string? DeliveryNote { get; set; }
        public string? ProofImageUrl { get; set; }
        public Guid? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ReliefRequest ReliefRequest { get; set; } = default!;
        public DistributionSession DistributionSession { get; set; } = default!;
        public ICollection<ReliefFulfillmentItem> Items { get; set; } = new List<ReliefFulfillmentItem>();
    }
}
