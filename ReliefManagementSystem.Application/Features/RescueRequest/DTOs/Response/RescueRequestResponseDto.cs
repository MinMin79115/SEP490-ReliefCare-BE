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

        public string RescueRequestType { get; set; } = null!;

        public string Description { get; set; } = null!;

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public string? Address { get; set; }

        public string ReporterFullName { get; set; } = null!;

        public string ReporterPhone { get; set; } = null!;

        public int? Priority { get; set; }

        public RescuePriorityLevel? PriorityLevel { get; set; }

        public string RescueRequestStatus { get; set; } = null!;

        public string DispatchMode { get; set; } = null!;

        public string? Note { get; set; }

        public string? WeatherCondition { get; set; }
        public double? WeatherTempC { get; set; }
        public double? WeatherWindKph { get; set; }
        public double? WeatherPrecipMm { get; set; }
        public double? WeatherVisibilityKm { get; set; }
        public int? WeatherRiskScore { get; set; }
        public string? WeatherRiskLevel { get; set; }
        public DateTime? WeatherObservedAt { get; set; }

        public Guid? CampaignId { get; set; }

        public string? CampaignName { get; set; }

        public double? StationToRequestDistanceKm { get; set; }

        public int? StationToRequestDurationMinutes { get; set; }

        public int? StationToRequestDistanceMeters { get; set; }

        public int? StationToRequestDurationSeconds { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public List<AttachmentResponseDto> Attachments { get; set; } = new();

        public List<RescueRequestPriorityDto> PriorityDetails { get; set; } = new();

        public List<RescueOperationDto> RescueOperations { get; set; } = new();

        public List<RequestVerificationDto> Verifications { get; set; } = new();

        public AssignedRescueTeamDto? AssignedRescueTeam { get; set; }

        public List<RescueOperationSupplyDto> Supplies { get; set; } = new();
    }

    /// <summary>DTO cho attachment</summary>
    public class AttachmentResponseDto
    {
        public Guid AttachmentId { get; set; }

        public string FileUrl { get; set; } = null!;

        public string ContentType { get; set; } = null!;

        public string AttachmentType { get; set; } = null!;

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

        public Guid? TeamId { get; set; }

        public Guid? VehicleId { get; set; }

        public string? TeamName { get; set; }

        public string? VehicleName { get; set; }

        public string? VehicleLicensePlate { get; set; }

        public List<AssignedVehicleDto> Vehicles { get; set; } = new();

        public List<RescueOperationSupplyDto> Supplies { get; set; } = new();

        public string? StationName { get; set; }

        public string? Status { get; set; }

        public DateTime StartedAt { get; set; }

        public DateTime? EndedAt { get; set; }
    }

    public class AssignedRescueTeamDto
    {
        public Guid RescueOperationId { get; set; }
        public Guid TeamId { get; set; }
        public string TeamName { get; set; } = null!;
        public Guid? VehicleId { get; set; }
        public string? VehicleName { get; set; }
        public string? VehicleLicensePlate { get; set; }
        public List<AssignedVehicleDto> Vehicles { get; set; } = new();
        public List<RescueOperationSupplyDto> Supplies { get; set; } = new();
        public string OperationStatus { get; set; } = null!;
        public double? CurrentLatitude { get; set; }
        public double? CurrentLongitude { get; set; }
        public DateTime? LastTrackedAt { get; set; }
        public int? EstimatedMinutesToArrival { get; set; }
        public double? DistanceKmToVictim { get; set; }
        public string? RoutePolyline { get; set; }
        public double? TotalDistanceKm { get; set; }
        public int? TotalEstimatedMinutes { get; set; }
    }

    public class AssignedVehicleDto
    {
        public Guid VehicleId { get; set; }
        public string? VehicleName { get; set; }
        public string? VehicleLicensePlate { get; set; }
        public bool IsPrimary { get; set; }
    }

    public class RescueOperationSupplyDto
    {
        public Guid RescueOperationSupplyId { get; set; }
        public Guid RescueOperationId { get; set; }
        public Guid SourceInventoryId { get; set; }
        public string? SourceInventoryName { get; set; }
        public Guid SupplyItemId { get; set; }
        public string? SupplyItemName { get; set; }
        public int Quantity { get; set; }
        public string? Unit { get; set; }
        public string? Notes { get; set; }
        public Guid? InventoryTransactionId { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid? CreatedBy { get; set; }
    }
}
