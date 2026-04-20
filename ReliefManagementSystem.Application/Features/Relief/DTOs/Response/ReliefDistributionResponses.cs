using ReliefManagementSystem.Domain.Enum;
using ReliefManagementSystem.Application.Common.Models;

namespace ReliefManagementSystem.Application.Features.Relief.DTOs.Response
{
    public class CampaignHouseholdResponse
    {
        public Guid CampaignHouseholdId { get; set; }
        public Guid CampaignId { get; set; }
        public Guid? DistributionPointId { get; set; }
        public Guid? CampaignTeamId { get; set; }
        public string HouseholdCode { get; set; } = string.Empty;
        public string HeadOfHouseholdName { get; set; } = string.Empty;
        public string? ContactPhone { get; set; }
        public string? Address { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int HouseholdSize { get; set; }
        public bool IsIsolated { get; set; }
        public DeliveryMode DeliveryMode { get; set; }
        public HouseholdFulfillmentStatus FulfillmentStatus { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class HouseholdChecklistItemResponse
    {
        public Guid HouseholdDeliveryId { get; set; }
        public Guid CampaignId { get; set; }
        public Guid CampaignHouseholdId { get; set; }
        public string HouseholdCode { get; set; } = string.Empty;
        public string HeadOfHouseholdName { get; set; } = string.Empty;
        public Guid? CampaignTeamId { get; set; }
        public Guid? DistributionPointId { get; set; }
        public Guid ReliefPackageDefinitionId { get; set; }
        public DeliveryMode DeliveryMode { get; set; }
        public HouseholdFulfillmentStatus Status { get; set; }
        public DateTime ScheduledAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public string? Notes { get; set; }
        public int ProofCount { get; set; }
    }

    public class DistributionPointResponse
    {
        public Guid DistributionPointId { get; set; }
        public Guid CampaignId { get; set; }
        public Guid ReliefStationId { get; set; }
        public Guid? CampaignTeamId { get; set; }
        public Guid? LocationId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Address { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DeliveryMode DeliveryMode { get; set; }
        public DateTime StartsAt { get; set; }
        public DateTime? EndsAt { get; set; }
        public bool IsActive { get; set; }
    }

    public class ReliefPackageDefinitionResponse
    {
        public Guid ReliefPackageDefinitionId { get; set; }
        public Guid CampaignId { get; set; }
        public Guid OutputSupplyItemId { get; set; }
        public string OutputSupplyItemName { get; set; } = string.Empty;
        public string OutputUnit { get; set; } = string.Empty;
        public decimal CashSupportAmount { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsDefault { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<ReliefPackageDefinitionItemResponse> Items { get; set; } = [];
    }

    public class ReliefPackageDefinitionItemResponse
    {
        public Guid ReliefPackageDefinitionItemId { get; set; }
        public Guid SupplyItemId { get; set; }
        public string SupplyItemName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string Unit { get; set; } = string.Empty;
    }

    public class HouseholdDeliveryResponse
    {
        public Guid HouseholdDeliveryId { get; set; }
        public Guid CampaignId { get; set; }
        public Guid CampaignHouseholdId { get; set; }
        public Guid? DistributionPointId { get; set; }
        public string? DistributionPointName { get; set; }
        public Guid? CampaignTeamId { get; set; }
        public string? CampaignTeamName { get; set; }
        public Guid ReliefPackageDefinitionId { get; set; }
        public string ReliefPackageDefinitionName { get; set; } = string.Empty;
        public Guid? DeliveredByUserId { get; set; }
        public DeliveryMode DeliveryMode { get; set; }
        public HouseholdFulfillmentStatus Status { get; set; }
        public decimal CashSupportAmount { get; set; }
        public DateTime ScheduledAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<HouseholdDeliveryProofResponse> Proofs { get; set; } = [];
    }

    public class HouseholdDeliveryProofResponse
    {
        public Guid HouseholdDeliveryProofId { get; set; }
        public string FileUrl { get; set; } = string.Empty;
        public string? FileType { get; set; }
        public string? Note { get; set; }
        public DateTime CapturedAt { get; set; }
        public Guid? CapturedByUserId { get; set; }
    }

    public class BatchCompleteHouseholdDeliveryItemResponse
    {
        public Guid HouseholdDeliveryId { get; set; }
        public bool IsSuccess { get; set; }
        public string? Error { get; set; }
        public HouseholdDeliveryResponse? Delivery { get; set; }
    }

    public class BatchCompleteHouseholdDeliveryResponse
    {
        public int TotalRequested { get; set; }
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
        public List<BatchCompleteHouseholdDeliveryItemResponse> Items { get; set; } = [];
    }

    public class SupplyShortageRequestResponse
    {
        public Guid SupplyShortageRequestId { get; set; }
        public Guid CampaignId { get; set; }
        public Guid? DistributionPointId { get; set; }
        public Guid? CampaignTeamId { get; set; }
        public Guid RequestedByUserId { get; set; }
        public SupplyShortageRequestStatus Status { get; set; }
        public string? Reason { get; set; }
        public DateTime RequestedAt { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public Guid? ReviewedByUserId { get; set; }
        public string? ReviewNote { get; set; }
        public List<SupplyShortageRequestItemResponse> Items { get; set; } = [];
    }

    public class SupplyShortageRequestItemResponse
    {
        public Guid SupplyShortageRequestItemId { get; set; }
        public Guid SupplyItemId { get; set; }
        public string SupplyItemName { get; set; } = string.Empty;
        public int QuantityRequested { get; set; }
        public int? QuantityApproved { get; set; }
        public string? Note { get; set; }
    }

    public class ReliefPackageAssemblyAvailabilityItemResponse
    {
        public Guid SupplyItemId { get; set; }
        public string SupplyItemName { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public int RequiredPerPackage { get; set; }
        public int AvailableQuantity { get; set; }
        public int MaxAssemblableByItem { get; set; }
    }

    public class ReliefPackageAssemblyAvailabilityResponse
    {
        public Guid CampaignId { get; set; }
        public Guid? CampaignInventoryId { get; set; }
        public Guid ReliefStationId { get; set; }
        public Guid InventoryId { get; set; }
        public Guid ReliefPackageDefinitionId { get; set; }
        public Guid OutputSupplyItemId { get; set; }
        public string OutputSupplyItemName { get; set; } = string.Empty;
        public string OutputUnit { get; set; } = string.Empty;
        public int MaxAssemblableQuantity { get; set; }
        public List<ReliefPackageAssemblyAvailabilityItemResponse> Components { get; set; } = [];
    }

    public class ReliefPackageAssemblyConsumeItemResponse
    {
        public Guid SupplyItemId { get; set; }
        public string SupplyItemName { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public int QuantityConsumed { get; set; }
    }

    public class ReliefPackageAssemblyResponse
    {
        public Guid ReliefPackageAssemblyId { get; set; }
        public Guid CampaignId { get; set; }
        public Guid ReliefStationId { get; set; }
        public Guid InventoryId { get; set; }
        public Guid ReliefPackageDefinitionId { get; set; }
        public Guid OutputSupplyItemId { get; set; }
        public string OutputSupplyItemName { get; set; } = string.Empty;
        public string OutputUnit { get; set; } = string.Empty;
        public int QuantityCreated { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? Notes { get; set; }
        public List<ReliefPackageAssemblyConsumeItemResponse> Details { get; set; } = [];
    }
}
