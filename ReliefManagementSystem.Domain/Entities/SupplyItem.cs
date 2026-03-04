using Microsoft.EntityFrameworkCore;
using ReliefManagementSystem.Domain.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ReliefManagementSystem.Domain.Entities;

public partial class SupplyItem
{
    [Key]
    public Guid SupplyItemId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public SupplyCategory Category { get; set; }

    public string Unit { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<InventoryStock> InventoryStocks { get; set; } = new List<InventoryStock>();

    public virtual ICollection<InventoryTransactionItem> InventoryTransactionItems { get; set; } = new List<InventoryTransactionItem>();

    public virtual ICollection<SupplyAllocationItem> SupplyAllocationItems { get; set; } = new List<SupplyAllocationItem>();
}
