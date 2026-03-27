using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Domain.Entities
{
    public class TeamTrackingPoint
    {
        public Guid TeamTrackingPointId { get; set; }

        public Guid TeamId { get; set; }
        public Team Team { get; set; } = null!;

        public Guid? RescueBatchId { get; set; }
        public RescueBatch? RescueBatch { get; set; }

        public Guid? RescueOperationId { get; set; }
        public RescueOperation? RescueOperation { get; set; }

        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double? AccuracyMeters { get; set; }
        public double? SpeedKph { get; set; }
        public double? HeadingDegree { get; set; }
        public TeamTrackingSource Source { get; set; } = TeamTrackingSource.MobileGps;
        public DateTime CapturedAtUtc { get; set; }
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public string? Note { get; set; }
    }
}
