using Microsoft.EntityFrameworkCore;
using ReliefManagementSystem.Domain.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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

    [NotMapped]
    public InventoryStatus InventoryStatus
    {
        get
        {
            if (MaximumStockLevel <= 0)
                return InventoryStatus.Critical;

            var percentage =
                (decimal)CurrentQuantity / MaximumStockLevel * 100;

            if (percentage >= 100) return InventoryStatus.Full;
            if (percentage >= 50) return InventoryStatus.Safe;
            if (percentage >= 15) return InventoryStatus.NeedRestock;
            return InventoryStatus.Critical;
        }
    }

    public virtual Inventory Inventory { get; set; } = null!;

    public virtual SupplyItem SupplyItem { get; set; } = null!;
}
