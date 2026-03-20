using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Application.Features.SupplyAllocation.DTOs.Response
{
    /// <summary>Full detail response for a supply allocation including all line items.</summary>
    public class SupplyAllocationResponse
    {
        public Guid AllocationId { get; set; }
        public Guid CampaignId { get; set; }
        public Guid SourceInventoryId { get; set; }
        public string SourceInventoryName { get; set; } = null!;
        public string ReliefStationName { get; set; } = null!;
        public SupplyAllocationStatus Status { get; set; }
        public string StatusName => Status.ToString();
        public DateTime AllocatedAt { get; set; }
        public IReadOnlyList<AllocationItemResponse> Items { get; set; } = [];
    }

    /// <summary>Summary response for list views (no line items).</summary>
    public class SupplyAllocationSummaryResponse
    {
        public Guid AllocationId { get; set; }
        public Guid CampaignId { get; set; }
        public Guid SourceInventoryId { get; set; }
        public string SourceInventoryName { get; set; } = null!;
        public SupplyAllocationStatus Status { get; set; }
        public string StatusName => Status.ToString();
        public int TotalItems { get; set; }
        public DateTime AllocatedAt { get; set; }
    }

    /// <summary>One resolved line item in an allocation response.</summary>
    public class AllocationItemResponse
    {
        public Guid AllocationItemId { get; set; }
        public Guid SupplyItemId { get; set; }
        public string SupplyItemName { get; set; } = null!;
        public string SupplyItemUnit { get; set; } = null!;
        public int Quantity { get; set; }
    }
}
