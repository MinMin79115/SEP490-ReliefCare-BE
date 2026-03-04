using Microsoft.EntityFrameworkCore;
using ReliefManagementSystem.Domain.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ReliefManagementSystem.Domain.Entities;

[Index("CampaignId", Name = "IX_SupplyAllocations_CampaignId")]
[Index("SourceInventoryId", Name = "IX_SupplyAllocations_SourceInventoryId")]
public partial class SupplyAllocation
{
    [Key]
    public Guid AllocationId { get; set; }

    public Guid CampaignId { get; set; }

    public Guid SourceInventoryId { get; set; }

    public DateTime AllocatedAt { get; set; }

    public SupplyAllocationStatus Status { get; set; }

    public virtual Campaign Campaign { get; set; } = null!;

    public virtual Inventory SourceInventory { get; set; } = null!;

    public virtual ICollection<SupplyAllocationItem> Items { get; set; } = new List<SupplyAllocationItem>();
}
