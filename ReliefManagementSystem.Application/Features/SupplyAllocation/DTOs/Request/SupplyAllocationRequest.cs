using ReliefManagementSystem.Domain.Enum;
using System.ComponentModel.DataAnnotations;

namespace ReliefManagementSystem.Application.Features.SupplyAllocation.DTOs.Request
{
    /// <summary>
    /// Request model to create a new supply allocation (Pending status by default).
    /// </summary>
    public class CreateSupplyAllocationRequest
    {
        [Required(ErrorMessage = "CampaignId is required.")]
        public Guid CampaignId { get; set; }

        [Required(ErrorMessage = "SourceInventoryId is required.")]
        public Guid SourceInventoryId { get; set; }

        /// <summary>Line items to allocate. At least one required.</summary>
        [Required]
        [MinLength(1, ErrorMessage = "At least one item is required.")]
        public List<AllocationItemRequest> Items { get; set; } = [];
    }

    /// <summary>One supply item with quantity to allocate.</summary>
    public class AllocationItemRequest
    {
        [Required(ErrorMessage = "SupplyItemId is required.")]
        public Guid SupplyItemId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; set; }
    }

    /// <summary>
    /// Request model to transition the allocation status.
    /// Valid transitions: Pending→Approved, Pending→Cancelled, Approved→Delivered, Approved→Cancelled.
    /// </summary>
    public class UpdateAllocationStatusRequest
    {
        [Required(ErrorMessage = "Status is required.")]
        public SupplyAllocationStatus Status { get; set; }
    }
}
