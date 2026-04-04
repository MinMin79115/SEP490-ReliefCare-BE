using ReliefManagementSystem.Domain.Enum;
using System.ComponentModel.DataAnnotations;

namespace ReliefManagementSystem.Application.Features.DistributionSession.DTOs.Request
{
    public class CreateDistributionSessionRequest
    {
        [Required]
        public Guid CampaignId { get; set; }

        [Required]
        public Guid ReliefStationId { get; set; }

        [Required]
        public DistributionSessionMode Mode { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public DateTime ScheduledStartAt { get; set; }

        public DateTime? ScheduledEndAt { get; set; }

        [MaxLength(200)]
        public string? LocationName { get; set; }

        [MaxLength(500)]
        public string? Address { get; set; }

        [Range(-90, 90)]
        public double? Latitude { get; set; }

        [Range(-180, 180)]
        public double? Longitude { get; set; }

        [Range(0, double.MaxValue)]
        public double? RadiusMeters { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }
    }
}
