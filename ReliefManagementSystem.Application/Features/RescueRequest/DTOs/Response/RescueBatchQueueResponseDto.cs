using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Application.Features.RescueRequest.DTOs.Response
{
    public class RescueBatchQueueResponseDto
    {
        public Guid RescueBatchId { get; set; }
        public Guid TeamId { get; set; }
        public bool IsActive { get; set; }
        public RescueBatchStatus Status { get; set; }
        public string? RoutePolyline { get; set; }
        public double? TotalDistanceKm { get; set; }
        public int? EstimatedMinutes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ClosedAt { get; set; }
        public List<RescueBatchQueueItemDto> Items { get; set; } = new();
    }

    public class RescueBatchQueueItemDto
    {
        public Guid RescueBatchItemId { get; set; }
        public Guid RescueRequestId { get; set; }
        public int SequenceOrder { get; set; }
        public bool IsAutoAssigned { get; set; }
        public double? DistanceKm { get; set; }
        public int? EstimatedMinutes { get; set; }
        public RescueBatchItemStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
