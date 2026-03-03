using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ReliefManagementSystem.Domain.Entities;

[Index("InventoryId", "SupplyItemId", Name = "IX_InventoryStocks_InventoryId_SupplyItemId", IsUnique = true)]
[Index("SupplyItemId", Name = "IX_InventoryStocks_SupplyItemId")]
public partial class InventoryStock
{
    [Key]
    public Guid InventoryStockId { get; set; }

    public Guid InventoryId { get; set; }

    public Guid SupplyItemId { get; set; }

    public int CurrentQuantity { get; set; }

    public int MinimumStockLevel { get; set; }

    public int MaximumStockLevel { get; set; }

    [ForeignKey("InventoryId")]
    [InverseProperty("InventoryStocks")]
    public virtual Inventory Inventory { get; set; } = null!;

    [ForeignKey("SupplyItemId")]
    [InverseProperty("InventoryStocks")]
    public virtual SupplyItem SupplyItem { get; set; } = null!;
}
