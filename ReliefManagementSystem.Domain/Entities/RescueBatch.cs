using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Domain.Entities
{
    public class RescueBatch
    {
        public Guid RescueBatchId { get; set; }

        public Guid TeamId { get; set; }
        public Team Team { get; set; } = null!;

        public bool IsActive { get; set; } = true;
        public RescueBatchStatus Status { get; set; } = RescueBatchStatus.Active;

        public string? RoutePolyline { get; set; }
        public double? TotalDistanceKm { get; set; }
        public int? EstimatedMinutes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ClosedAt { get; set; }

        public ICollection<RescueBatchItem> Items { get; set; } = new List<RescueBatchItem>();
        public ICollection<TeamTrackingPoint> TrackingPoints { get; set; } = new List<TeamTrackingPoint>();
    }
}
