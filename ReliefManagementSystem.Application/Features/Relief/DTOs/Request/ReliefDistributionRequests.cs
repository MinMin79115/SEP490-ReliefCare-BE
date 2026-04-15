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

        [Range(1, int.MaxValue)]
        public int HouseholdSize { get; set; }

        public bool IsIsolated { get; set; }
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

    public class CreateReliefPackageDefinitionRequest
    {
        [Required]
        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        public bool IsDefault { get; set; }
        public bool IsActive { get; set; } = true;

        [Required]
        [MinLength(1)]
        public List<ReliefPackageDefinitionItemRequest> Items { get; set; } = [];
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

    public class CompleteHouseholdDeliveryRequest
    {
        public Guid? ReliefPackageDefinitionId { get; set; }
        public Guid? CampaignTeamId { get; set; }
        public string? Notes { get; set; }
        public string? ProofNote { get; set; }
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
