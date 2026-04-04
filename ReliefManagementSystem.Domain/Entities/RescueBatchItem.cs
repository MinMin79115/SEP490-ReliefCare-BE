using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Domain.Entities
{
    public class RescueBatchItem
    {
        public Guid RescueBatchItemId { get; set; }

        public Guid RescueBatchId { get; set; }
        public RescueBatch RescueBatch { get; set; } = null!;

        public Guid RescueRequestId { get; set; }
        public RescueRequest RescueRequest { get; set; } = null!;

        public int SequenceOrder { get; set; }
        public bool IsAutoAssigned { get; set; }

        public double? DistanceKm { get; set; }
        public int? EstimatedMinutes { get; set; }

        public RescueBatchItemStatus Status { get; set; } = RescueBatchItemStatus.Pending;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
