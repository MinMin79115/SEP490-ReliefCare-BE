using Microsoft.EntityFrameworkCore;
using ReliefManagementSystem.Domain.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ReliefManagementSystem.Domain.Entities;

[Index("ReliefStationId", Name = "IX_Inventories_ReliefStationId")]
public partial class Inventory
{
    [Key]
    public Guid InventoryId { get; set; }

    public Guid ReliefStationId { get; set; }

    public InventoryLevel Level { get; set; }

    public EntityStatus Status { get; set; }

    public virtual ICollection<InventoryStock> InventoryItems { get; set; } = new List<InventoryStock>();

    public virtual ICollection<InventoryTransaction> InventoryTransactions { get; set; } = new List<InventoryTransaction>();

    [ForeignKey("ReliefStationId")]
    public virtual ReliefStation ReliefStation { get; set; } = null!;

    public virtual ICollection<SupplyAllocation> SupplyAllocations { get; set; } = new List<SupplyAllocation>();
}
