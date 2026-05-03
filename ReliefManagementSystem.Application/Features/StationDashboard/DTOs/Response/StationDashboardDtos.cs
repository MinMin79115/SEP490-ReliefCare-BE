namespace ReliefManagementSystem.Application.Features.StationDashboard.DTOs.Response
{
    public class StationOverviewResponseDto
    {
        public Guid StationId { get; set; }
        public string StationName { get; set; } = string.Empty;
        public int PendingRescueRequests { get; set; }
        public int VerifiedRescueRequests { get; set; }
        public int AssignedRescueRequests { get; set; }
        public int InProgressRescueRequests { get; set; }
        public int CompletedToday { get; set; }
        public int ActiveTeams { get; set; }
        public int AvailableVehicles { get; set; }
        public int BusyVehicles { get; set; }
        public int UnreadNotifications { get; set; }
        public int LowStockItems { get; set; }
        public int PendingShortageRequests { get; set; }
    }

    public class RescueRequestStatusSummaryDto
    {
        public int Total { get; set; }
        public int Pending { get; set; }
        public int Verified { get; set; }
        public int Assigned { get; set; }
        public int InProgress { get; set; }
        public int Completed { get; set; }
        public int Cancelled { get; set; }
    }

    public class TeamPerformanceResponseDto
    {
        public List<TeamPerformanceItemDto> Data { get; set; } = new();
    }

    public class ReliefTeamMissionSnapshotResponseDto
    {
        public List<ReliefTeamMissionSnapshotItemDto> Data { get; set; } = new();
    }

    public class ReliefTeamMissionSnapshotItemDto
    {
        public Guid TeamId { get; set; }
        public Guid CampaignTeamId { get; set; }
        public Guid CampaignId { get; set; }
        public string TeamName { get; set; } = string.Empty;
        public string TeamType { get; set; } = string.Empty;
        public string CampaignName { get; set; } = string.Empty;
        public string CampaignStatus { get; set; } = string.Empty;
        public string CampaignTeamStatus { get; set; } = string.Empty;
        public int TotalTasks { get; set; }
        public int PlannedTasks { get; set; }
        public int InProgressTasks { get; set; }
        public int BlockedTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int CancelledTasks { get; set; }
        public int TotalSubTasks { get; set; }
        public int AssignedSubTasks { get; set; }
        public int InProgressSubTasks { get; set; }
        public int CompletedSubTasks { get; set; }
        public int FailedSubTasks { get; set; }
        public int CancelledSubTasks { get; set; }
        public int HouseholdCount { get; set; }
        public int PendingHouseholdCount { get; set; }
        public int DeliveredHouseholdCount { get; set; }
        public int TotalDeliveryCount { get; set; }
        public int PendingDeliveryCount { get; set; }
        public int DeliveredDeliveryCount { get; set; }
        public string? DefaultReliefPackageName { get; set; }
        public DateTime? LastTaskUpdatedAt { get; set; }
    }

    public class TeamPerformanceItemDto
    {
        public Guid TeamId { get; set; }
        public string TeamName { get; set; } = string.Empty;
        public string TeamType { get; set; } = string.Empty;
        public int AssignedRequests { get; set; }
        public bool ActiveBatch { get; set; }
        public int InProgressRequests { get; set; }
        public int CompletedRequests { get; set; }
        public DateTime? LastTrackedAt { get; set; }
    }

    public class VehicleSummaryResponseDto
    {
        public int Total { get; set; }
        public int Available { get; set; }
        public int Busy { get; set; }
        public List<VehicleTypeSummaryDto> ByType { get; set; } = new();
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 0;
        public int TotalPages { get; set; } = 1;
        public bool HasPrevious { get; set; }
        public bool HasNext { get; set; }
    }

    public class VehicleTypeSummaryDto
    {
        public string VehicleTypeName { get; set; } = string.Empty;
        public int Total { get; set; }
        public int Available { get; set; }
        public int Busy { get; set; }
    }

    public class StationAlertsSummaryDto
    {
        public int UnreadNotifications { get; set; }
        public int PendingVolunteerApplications { get; set; }
        public int PendingJoinRequests { get; set; }
        public int PendingShortageRequests { get; set; }
        public int CriticalStockItems { get; set; }
        public int VehiclesUnavailable { get; set; }
    }

    public class InventorySummaryResponseDto
    {
        public int InventoryCount { get; set; }
        public int TotalStockItems { get; set; }
        public int SafeItems { get; set; }
        public int NeedRestockItems { get; set; }
        public int CriticalItems { get; set; }
        public List<CriticalStockItemDto> TopCriticalItems { get; set; } = new();
    }

    public class CriticalStockItemDto
    {
        public Guid SupplyItemId { get; set; }
        public string SupplyItemName { get; set; } = string.Empty;
        public int CurrentQuantity { get; set; }
        public int MinimumStockLevel { get; set; }
    }

    public class RescueRequestTrendResponseDto
    {
        public string GroupBy { get; set; } = "day";
        public List<RescueRequestTrendItemDto> Data { get; set; } = new();
    }

    public class RescueRequestTypeSummaryResponseDto
    {
        public int Total { get; set; }
        public int Normal { get; set; }
        public int Emergency { get; set; }
    }

    public class RescueRequestTrendItemDto
    {
        public string Label { get; set; } = string.Empty;
        public int Created { get; set; }
        public int Assigned { get; set; }
        public int Completed { get; set; }
    }

    public class ActiveDispatchSnapshotResponseDto
    {
        public List<ActiveDispatchItemDto> ActiveOperations { get; set; } = new();
    }

    public class RescueRequestLocationsResponseDto
    {
        public List<RescueRequestLocationItemDto> Items { get; set; } = new();
    }

    public class RescueRequestLocationItemDto
    {
        public Guid RequestId { get; set; }
        public string? Address { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string RescueRequestType { get; set; } = string.Empty;
        public string RescueRequestStatus { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class ActiveDispatchItemDto
    {
        public Guid RequestId { get; set; }
        public Guid OperationId { get; set; }
        public string TeamName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? Address { get; set; }
        public DateTime? LastTrackedAt { get; set; }
        public List<SimpleAssignedVehicleDto> Vehicles { get; set; } = new();
    }

    public class SimpleAssignedVehicleDto
    {
        public Guid VehicleId { get; set; }
        public string? VehicleName { get; set; }
        public string? VehicleLicensePlate { get; set; }
        public bool IsPrimary { get; set; }
    }
}
