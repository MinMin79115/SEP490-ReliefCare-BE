using ReliefManagementSystem.Application.Common.Models;

namespace ReliefManagementSystem.Application.Features.StationReports.DTOs.Response
{
    public class RescueRequestReportItemDto
    {
        public Guid RequestId { get; set; }
        public string? Address { get; set; }
        public string? RescueRequestType { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? TeamName { get; set; }
        public string? PrimaryVehicle { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class TeamWorkloadReportItemDto
    {
        public Guid TeamId { get; set; }
        public string TeamName { get; set; } = string.Empty;
        public int AssignedRequests { get; set; }
        public int CompletedRequests { get; set; }
        public int ActiveBatchCount { get; set; }
        public int MemberCount { get; set; }
        public int PendingHouseholdCount { get; set; }
        public int DeliveredHouseholdCount { get; set; }
        public int TotalDeliveryCount { get; set; }
        public int DeliveredDeliveryCount { get; set; }
    }

    public class VehicleUtilizationReportItemDto
    {
        public Guid VehicleId { get; set; }
        public string? VehicleName { get; set; }
        public string? VehicleLicensePlate { get; set; }
        public int BusyCount { get; set; }
        public int UsedInOperations { get; set; }
        public bool IsCurrentlyBusy { get; set; }
    }

    public class InventoryStockReportItemDto
    {
        public Guid InventoryStockId { get; set; }
        public string SupplyItemName { get; set; } = string.Empty;
        public int CurrentQuantity { get; set; }
        public int MinimumStockLevel { get; set; }
        public int MaximumStockLevel { get; set; }
        public string InventoryStatus { get; set; } = string.Empty;
    }

    public class ReliefDeliveryReportItemDto
    {
        public string HouseholdCode { get; set; } = string.Empty;
        public string HeadOfHouseholdName { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? TeamName { get; set; }
        public string DeliveryMode { get; set; } = string.Empty;
        public string FulfillmentStatus { get; set; } = string.Empty;
    }

    public class ReliefMissionReportRowDto
    {
        public Guid CampaignId { get; set; }
        public string CampaignName { get; set; } = string.Empty;
        public string CampaignStatus { get; set; } = string.Empty;
        public Guid TeamId { get; set; }
        public string TeamName { get; set; } = string.Empty;
        public string TeamType { get; set; } = string.Empty;
        public Guid CampaignTeamId { get; set; }
        public string CampaignTeamStatus { get; set; } = string.Empty;
        public Guid CampaignTaskId { get; set; }
        public string CampaignTaskTitle { get; set; } = string.Empty;
        public string CampaignTaskStatus { get; set; } = string.Empty;
        public DateTime TaskStartDate { get; set; }
        public DateTime? TaskDueDate { get; set; }
        public int TotalSubTasks { get; set; }
        public int AssignedSubTasks { get; set; }
        public int InProgressSubTasks { get; set; }
        public int CompletedSubTasks { get; set; }
        public int FailedSubTasks { get; set; }
        public int CancelledSubTasks { get; set; }
        public DateTime? LastSubTaskUpdatedAt { get; set; }
    }

    public class ReliefMissionCampaignSummaryDto
    {
        public Guid CampaignId { get; set; }
        public string CampaignName { get; set; } = string.Empty;
        public string CampaignStatus { get; set; } = string.Empty;
        public int TeamCount { get; set; }
        public int TaskCount { get; set; }
        public int BlockedTaskCount { get; set; }
        public int TotalSubTaskCount { get; set; }
        public int CompletedSubTaskCount { get; set; }
        public int InProgressSubTaskCount { get; set; }
        public int FailedSubTaskCount { get; set; }
        public int CancelledSubTaskCount { get; set; }
    }
}
