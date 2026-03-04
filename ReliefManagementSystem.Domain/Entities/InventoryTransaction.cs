using Microsoft.EntityFrameworkCore;
using ReliefManagementSystem.Domain.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ReliefManagementSystem.Domain.Entities;

[Index("CreatedBy", Name = "IX_InventoryTransactions_CreatedBy")]
[Index("InventoryId", Name = "IX_InventoryTransactions_InventoryId")]
public partial class InventoryTransaction
{
    [Key]
    public Guid TransactionId { get; set; }

    public Guid InventoryId { get; set; }

    public string TransactionCode { get; set; } = null!;

    public TransactionType Type { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid CreatedBy { get; set; }

    public string? Notes { get; set; }

    public virtual ApplicationUser CreatedByUser { get; set; } = null!;

    public virtual Inventory Inventory { get; set; } = null!;

    public virtual ICollection<InventoryTransactionItem> Items { get; set; } = new List<InventoryTransactionItem>();
}
