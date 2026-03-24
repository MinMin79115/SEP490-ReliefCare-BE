using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Application.Features.Team.DTOs.Response
{
    public class TeamTrackingPointResponse
    {
        public Guid TeamTrackingPointId { get; set; }
        public Guid TeamId { get; set; }
        public Guid? RescueBatchId { get; set; }
        public Guid? RescueOperationId { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double? AccuracyMeters { get; set; }
        public double? SpeedKph { get; set; }
        public double? HeadingDegree { get; set; }
        public TeamTrackingSource Source { get; set; }
        public DateTime CapturedAtUtc { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public string? Note { get; set; }
    }
}
