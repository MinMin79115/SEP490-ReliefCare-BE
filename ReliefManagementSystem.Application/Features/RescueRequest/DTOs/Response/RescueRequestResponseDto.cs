using System;
using System.Collections.Generic;
using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Application.Features.RescueRequest.DTOs.Response
{
    /// <summary>DTO phản hồi thông tin yêu cầu cứu hộ</summary>
    public class RescueRequestResponseDto
    {
        public Guid RequestId { get; set; }

        public string DisasterType { get; set; } = null!;

        public string Description { get; set; } = null!;

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public string? Address { get; set; }

        public string ReporterFullName { get; set; } = null!;

        public string ReporterPhone { get; set; } = null!;

        public int? Priority { get; set; }

        public string RescueRequestStatus { get; set; } = null!;

        public string DispatchMode { get; set; } = null!;

        public string? Note { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public List<AttachmentResponseDto> Attachments { get; set; } = new();

        public List<RescueRequestPriorityDto> PriorityDetails { get; set; } = new();

        public List<RescueOperationDto> RescueOperations { get; set; } = new();
    }

    /// <summary>DTO cho attachment</summary>
    public class AttachmentResponseDto
    {
        public Guid AttachmentId { get; set; }

        public string FileUrl { get; set; } = null!;

        public string ContentType { get; set; } = null!;

        public DateTime UploadedAt { get; set; }
    }

    /// <summary>DTO cho priority criteria details</summary>
    public class RescueRequestPriorityDto
    {
        public string CriteriaName { get; set; } = null!;

        public int AppliedPoint { get; set; }

        public string Description { get; set; } = null!;
    }

    /// <summary>DTO cho rescue operation</summary>
    public class RescueOperationDto
    {
        public Guid RescueOperationId { get; set; }

        public string? StationName { get; set; }

        public DateTime StartedAt { get; set; }

        public DateTime? EndedAt { get; set; }
    }
}