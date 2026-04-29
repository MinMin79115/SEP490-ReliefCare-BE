using ReliefManagementSystem.Domain.Enum;
using System.ComponentModel.DataAnnotations;

namespace ReliefManagementSystem.Application.Features.Relief.DTOs.Request
{
    public class ImportCampaignHouseholdsRequest
    {
        [Required]
        [MinLength(1)]
        public List<ReliefHouseholdInputRequest> Households { get; set; } = [];
    }

    public class ReliefHouseholdInputRequest
    {
        [Required]
        [MaxLength(100)]
        public string HouseholdCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string HeadOfHouseholdName { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? ContactPhone { get; set; }

        [MaxLength(500)]
        public string? Address { get; set; }

        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public Guid? LocationId { get; set; }

        [Range(1, int.MaxValue)]
        public int HouseholdSize { get; set; }

        public bool IsIsolated { get; set; }
        [Range(0, 10)]
        public int? FloodSeverityLevel { get; set; }
        [Range(0, 10)]
        public int? IsolationSeverityLevel { get; set; }
        public bool RequiresBoat { get; set; }
        public bool RequiresLocalGuide { get; set; }
        public DeliveryMode? DeliveryMode { get; set; }
    }

    public class AssignHouseholdRequest
    {
        [Required]
        public DeliveryMode DeliveryMode { get; set; }

        public Guid? DistributionPointId { get; set; }
        public Guid? CampaignTeamId { get; set; }
        public Guid? ReliefPackageDefinitionId { get; set; }
        public DateTime? ScheduledAt { get; set; }
        public string? Notes { get; set; }
    }

    public class AssignIsolatedHouseholdTeamRequest
    {
        [Required]
        public Guid CampaignTeamId { get; set; }

        public Guid? ReliefPackageDefinitionId { get; set; }
        public DateTime? ScheduledAt { get; set; }
        public bool KeepDoorToDoor { get; set; } = true;
        public string? Notes { get; set; }
    }

    public class UpdateHouseholdDeliveryAssignmentRequest
    {
        [Required]
        public DeliveryMode DeliveryMode { get; set; }

        public Guid? DistributionPointId { get; set; }
        public Guid? CampaignTeamId { get; set; }
        public Guid? ReliefPackageDefinitionId { get; set; }
        public DateTime? ScheduledAt { get; set; }
        public string? Notes { get; set; }
    }

    public class BulkAssignIsolatedHouseholdsRequest
    {
        [Required]
        [MinLength(1)]
        public List<Guid> CampaignHouseholdIds { get; set; } = [];

        [Required]
        public Guid CampaignTeamId { get; set; }

        public Guid? ReliefPackageDefinitionId { get; set; }
        public DateTime? ScheduledAt { get; set; }
        public bool KeepDoorToDoor { get; set; } = true;
        public string? Notes { get; set; }
    }

    public class ReliefPagedQueryRequest
    {
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? Search { get; set; }
    }

    public class HouseholdQueryRequest : ReliefPagedQueryRequest
    {
        public HouseholdFulfillmentStatus? Status { get; set; }
        public DeliveryMode? DeliveryMode { get; set; }
        public Guid? DistributionPointId { get; set; }
        public Guid? CampaignTeamId { get; set; }
        public bool? IsIsolated { get; set; }
        public bool? IsAssigned { get; set; }
        public bool? RequiresBoat { get; set; }
        public bool? RequiresLocalGuide { get; set; }
        public int? MinFloodSeverityLevel { get; set; }
        public int? MinIsolationSeverityLevel { get; set; }
        public bool? HasCoordinates { get; set; }
    }

    public class DeliveryQueryRequest : ReliefPagedQueryRequest
    {
        public HouseholdFulfillmentStatus? Status { get; set; }
        public Guid? CampaignTeamId { get; set; }
        public Guid? DistributionPointId { get; set; }
        public DeliveryMode? DeliveryMode { get; set; }
        public DateTime? ScheduledFrom { get; set; }
        public DateTime? ScheduledTo { get; set; }
    }

    public class TeamDeliveryWorklistQueryRequest : DeliveryQueryRequest
    {
        public bool? OnlyMyTeam { get; set; }
        public bool? PrioritizeIsolated { get; set; }
        public bool? IncludePendingOnly { get; set; } = true;
        public bool? RequiresBoat { get; set; }
        public bool? RequiresLocalGuide { get; set; }
        public int? MinFloodSeverityLevel { get; set; }
        public int? MinIsolationSeverityLevel { get; set; }
    }

    public class DistributionPointQueryRequest : ReliefPagedQueryRequest
    {
        public Guid? ReliefStationId { get; set; }
        public Guid? CampaignTeamId { get; set; }
        public bool? IsActive { get; set; }
        public DeliveryMode? DeliveryMode { get; set; }
    }

    public class ReliefPackageQueryRequest : ReliefPagedQueryRequest
    {
        public bool? IsActive { get; set; }
        public bool? IsDefault { get; set; }
    }

    public class UpdateCampaignHouseholdRequest
    {
        [MaxLength(100)]
        public string? HouseholdCode { get; set; }

        [MaxLength(255)]
        public string? HeadOfHouseholdName { get; set; }

        [MaxLength(50)]
        public string? ContactPhone { get; set; }

        [MaxLength(500)]
        public string? Address { get; set; }

        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public Guid? LocationId { get; set; }
        public int? HouseholdSize { get; set; }
        public bool? IsIsolated { get; set; }
        [Range(0, 10)]
        public int? FloodSeverityLevel { get; set; }
        [Range(0, 10)]
        public int? IsolationSeverityLevel { get; set; }
        public bool? RequiresBoat { get; set; }
        public bool? RequiresLocalGuide { get; set; }
        public DeliveryMode? DeliveryMode { get; set; }
        public Guid? DistributionPointId { get; set; }
        public Guid? CampaignTeamId { get; set; }
        public string? Notes { get; set; }
    }

    public class UpdateCampaignHouseholdStatusRequest
    {
        [Required]
        public HouseholdFulfillmentStatus Status { get; set; }

        public string? Notes { get; set; }
    }

    public class CreateDistributionPointRequest
    {
        [Required]
        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public Guid ReliefStationId { get; set; }

        public Guid? CampaignTeamId { get; set; }
        public Guid? LocationId { get; set; }

        [MaxLength(500)]
        public string? Address { get; set; }

        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public DeliveryMode DeliveryMode { get; set; } = DeliveryMode.PickupAtPoint;

        [Required]
        public DateTime StartsAt { get; set; }

        public DateTime? EndsAt { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class UpdateDistributionPointRequest
    {
        [MaxLength(255)]
        public string? Name { get; set; }
        public Guid? ReliefStationId { get; set; }
        public Guid? CampaignTeamId { get; set; }
        public Guid? LocationId { get; set; }

        [MaxLength(500)]
        public string? Address { get; set; }

        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public DeliveryMode? DeliveryMode { get; set; }
        public DateTime? StartsAt { get; set; }
        public DateTime? EndsAt { get; set; }
        public bool? IsActive { get; set; }
    }

    public class CreateReliefPackageDefinitionRequest
    {
        [Required]
        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        public Guid? OutputSupplyItemId { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? CashSupportAmount { get; set; }

        public bool IsDefault { get; set; }
        public bool IsActive { get; set; } = true;

        [Required]
        [MinLength(1)]
        public List<ReliefPackageDefinitionItemRequest> Items { get; set; } = [];
    }

    public class UpdateReliefPackageDefinitionRequest
    {
        [MaxLength(255)]
        public string? Name { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        public Guid? OutputSupplyItemId { get; set; }
        [Range(0, double.MaxValue)]
        public decimal? CashSupportAmount { get; set; }
        public bool? IsDefault { get; set; }
        public bool? IsActive { get; set; }
        public List<ReliefPackageDefinitionItemRequest>? Items { get; set; }
    }

    public class ReliefPackageDefinitionItemRequest
    {
        [Required]
        public Guid SupplyItemId { get; set; }

        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        [Required]
        [MaxLength(50)]
        public string Unit { get; set; } = string.Empty;
    }

    public class AssembleReliefPackageRequest
    {
        [Required]
        public Guid ReliefStationId { get; set; }

        [Required]
        public Guid InventoryId { get; set; }

        [Range(1, int.MaxValue)]
        public int QuantityToAssemble { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }
    }

    public class CompleteHouseholdDeliveryRequest
    {
        public Guid? ReliefPackageDefinitionId { get; set; }
        public Guid? CampaignTeamId { get; set; }
        [Range(0, double.MaxValue)]
        public decimal? CashSupportAmount { get; set; }
        public string? Notes { get; set; }
        public string? ProofNote { get; set; }

        [Required]
        [MaxLength(1000)]
        public string ProofFileUrl { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? ProofContentType { get; set; }
    }

    public class CompleteHouseholdDeliveryProofRequest
    {
        [Required]
        [MaxLength(1000)]
        public string FileUrl { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? FileType { get; set; }

        public string? Note { get; set; }
    }

    public class CompleteHouseholdDeliveryBatchItemRequest
    {
        [Required]
        public Guid HouseholdDeliveryId { get; set; }
        public Guid? ReliefPackageDefinitionId { get; set; }
        public Guid? CampaignTeamId { get; set; }
        [Range(0, double.MaxValue)]
        public decimal? CashSupportAmount { get; set; }
        public string? Notes { get; set; }
        [Required]
        [MinLength(1)]
        public List<CompleteHouseholdDeliveryProofRequest> Proofs { get; set; } = [];
    }

    public class CompleteHouseholdDeliveryBatchRequest
    {
        [Required]
        [MinLength(1)]
        public List<CompleteHouseholdDeliveryBatchItemRequest> Items { get; set; } = [];
    }

    public class CreateSupplyShortageRequest
    {
        public Guid? DistributionPointId { get; set; }
        public Guid? CampaignTeamId { get; set; }
        public string? Reason { get; set; }

        [Required]
        [MinLength(1)]
        public List<SupplyShortageItemRequest> Items { get; set; } = [];
    }

    public class SupplyShortageRequestQueryRequest : ReliefPagedQueryRequest
    {
        public SupplyShortageRequestStatus? Status { get; set; }
        public Guid? DistributionPointId { get; set; }
        public Guid? CampaignTeamId { get; set; }
        public Guid? RequestedByUserId { get; set; }
    }

    public class SupplyShortageItemRequest
    {
        [Required]
        public Guid SupplyItemId { get; set; }

        [Range(1, int.MaxValue)]
        public int QuantityRequested { get; set; }

        public string? Note { get; set; }
    }

    public class ReviewSupplyShortageRequest
    {
        public string? ReviewNote { get; set; }
        public List<ApprovedSupplyShortageItemRequest>? ApprovedItems { get; set; }
    }

    public class ApprovedSupplyShortageItemRequest
    {
        [Required]
        public Guid SupplyItemId { get; set; }

        [Range(0, int.MaxValue)]
        public int QuantityApproved { get; set; }
    }
}
