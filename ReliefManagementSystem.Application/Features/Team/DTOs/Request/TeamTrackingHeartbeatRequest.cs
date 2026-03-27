using ReliefManagementSystem.Domain.Enum;
using System.ComponentModel.DataAnnotations;

namespace ReliefManagementSystem.Application.Features.Team.DTOs.Request
{
    public class TeamTrackingHeartbeatRequest
    {
        [Range(-90, 90)]
        public double Latitude { get; set; }

        [Range(-180, 180)]
        public double Longitude { get; set; }

        public double? AccuracyMeters { get; set; }
        public double? SpeedKph { get; set; }
        public double? HeadingDegree { get; set; }
        public TeamTrackingSource Source { get; set; } = TeamTrackingSource.MobileGps;
        public DateTime CapturedAtUtc { get; set; }

        public Guid? RescueBatchId { get; set; }
        public Guid? RescueOperationId { get; set; }
        public string? Note { get; set; }
    }
}
