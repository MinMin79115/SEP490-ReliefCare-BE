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
        public Guid? VehicleId { get; set; }
        public string? VehicleName { get; set; }
        public string? VehicleLicensePlate { get; set; }
        public string? DisasterType { get; set; }
        public string? RescueRequestType { get; set; }
        public string? RescueRequestStatus { get; set; }
        public string? Description { get; set; }
        public string? Address { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string? ReporterFullName { get; set; }
        public string? ReporterPhone { get; set; }
        public int? PriorityPoint { get; set; }
        public RescuePriorityLevel? PriorityLevel { get; set; }
        public int SequenceOrder { get; set; }
        public bool IsAutoAssigned { get; set; }
        public double? DistanceKm { get; set; }
        public int? EstimatedMinutes { get; set; }
        public RescueBatchItemStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
