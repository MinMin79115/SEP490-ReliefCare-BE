using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

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

    public int Status { get; set; }

    [ForeignKey("CampaignId")]
    [InverseProperty("SupplyAllocations")]
    public virtual Campaign Campaign { get; set; } = null!;

    [ForeignKey("SourceInventoryId")]
    [InverseProperty("SupplyAllocations")]
    public virtual Inventory SourceInventory { get; set; } = null!;

    [InverseProperty("Allocation")]
    public virtual ICollection<SupplyAllocationItem> SupplyAllocationItems { get; set; } = new List<SupplyAllocationItem>();
}
