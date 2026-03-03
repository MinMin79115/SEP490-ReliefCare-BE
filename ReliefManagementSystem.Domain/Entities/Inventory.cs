using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ReliefManagementSystem.Domain.Entities;

[Index("ReliefStationId", Name = "IX_Inventories_ReliefStationId")]
public partial class Inventory
{
    [Key]
    public Guid InventoryId { get; set; }

    public Guid ReliefStationId { get; set; }

    public int Level { get; set; }

    public int Status { get; set; }

    [InverseProperty("Inventory")]
    public virtual ICollection<InventoryStock> InventoryStocks { get; set; } = new List<InventoryStock>();

    [InverseProperty("Inventory")]
    public virtual ICollection<InventoryTransaction> InventoryTransactions { get; set; } = new List<InventoryTransaction>();

    [ForeignKey("ReliefStationId")]
    [InverseProperty("Inventories")]
    public virtual ReliefStation ReliefStation { get; set; } = null!;

    [InverseProperty("SourceInventory")]
    public virtual ICollection<SupplyAllocation> SupplyAllocations { get; set; } = new List<SupplyAllocation>();
}
