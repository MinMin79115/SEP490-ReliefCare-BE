using ReliefManagementSystem.Domain.Enum;
using ReliefManagementSystem.Application.Features.RescueRequest.DTOs.Response;

namespace ReliefManagementSystem.Application.Features.ReliefRequest.DTOs.Response
{
    public class ReliefRequestResponseDto
    {
        public Guid RequestId { get; set; }
        public string Description { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double? Accuracy { get; set; }
        public string? Address { get; set; }
        public Guid? LocationId { get; set; }
        public string ReporterFullName { get; set; } = string.Empty;
        public string ReporterPhone { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public Guid? CampaignId { get; set; }
        public string? CampaignName { get; set; }
        public Guid? AssignedReliefStationId { get; set; }
        public string? AssignedReliefStationName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public List<ReliefNeedItemResponseDto> NeedItems { get; set; } = new();
        public List<ReliefAttachmentResponseDto> Attachments { get; set; } = new();
        public List<RequestVerificationDto> Verifications { get; set; } = new();
    }

    public class ReliefNeedItemResponseDto
    {
        public Guid ReliefNeedItemId { get; set; }
        public ReliefNeedType NeedType { get; set; }
        public UrgencyLevel UrgencyLevel { get; set; }
        public int PeopleCount { get; set; }
        public string? Note { get; set; }
    }

    public class ReliefAttachmentResponseDto
    {
        public Guid AttachmentId { get; set; }
        public string FileUrl { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; }
    }
}
