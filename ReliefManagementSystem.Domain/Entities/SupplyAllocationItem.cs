using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ReliefManagementSystem.Domain.Entities;

[Index("AllocationId", Name = "IX_SupplyAllocationItems_AllocationId")]
[Index("SupplyItemId", Name = "IX_SupplyAllocationItems_SupplyItemId")]
public partial class SupplyAllocationItem
{
    [Key]
    public Guid AllocationItemId { get; set; }

    public Guid AllocationId { get; set; }

    public Guid SupplyItemId { get; set; }

    public int Quantity { get; set; }

    [ForeignKey("AllocationId")]
    [InverseProperty("SupplyAllocationItems")]
    public virtual SupplyAllocation Allocation { get; set; } = null!;

    [ForeignKey("SupplyItemId")]
    [InverseProperty("SupplyAllocationItems")]
    public virtual SupplyItem SupplyItem { get; set; } = null!;
}
