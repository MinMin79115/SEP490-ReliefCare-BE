using ReliefManagementSystem.Domain.Enum;
using System.ComponentModel.DataAnnotations;

namespace ReliefManagementSystem.Application.Features.ReliefRequest.DTOs.Request
{
    public class CreateReliefRequestDto
    {
        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Range(-90, 90)]
        public double Latitude { get; set; }

        [Required]
        [Range(-180, 180)]
        public double Longitude { get; set; }

        [Range(0, double.MaxValue)]
        public double? Accuracy { get; set; }

        [MaxLength(500)]
        public string? Address { get; set; }

        public Guid? LocationId { get; set; }

        [MaxLength(200)]
        public string? ReporterFullName { get; set; }

        [Required]
        [MaxLength(50)]
        public string ReporterPhone { get; set; } = string.Empty;

        [Required]
        [MinLength(1)]
        public List<ReliefNeedItemDto> NeedItems { get; set; } = new();

        public List<AttachmentDto>? Attachments { get; set; } = new();
    }

    public class ReliefNeedItemDto
    {
        [Required]
        public ReliefNeedType NeedType { get; set; }

        [Required]
        public UrgencyLevel UrgencyLevel { get; set; }

        [Range(1, int.MaxValue)]
        public int PeopleCount { get; set; }

        [MaxLength(500)]
        public string? Note { get; set; }
    }

    public class AttachmentDto
    {
        [Required]
        public string FileUrl { get; set; } = string.Empty;

        [Required]
        public string ContentType { get; set; } = string.Empty;
    }
}
