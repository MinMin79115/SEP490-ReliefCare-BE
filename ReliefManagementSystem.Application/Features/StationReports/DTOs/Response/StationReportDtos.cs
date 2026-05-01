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
}
