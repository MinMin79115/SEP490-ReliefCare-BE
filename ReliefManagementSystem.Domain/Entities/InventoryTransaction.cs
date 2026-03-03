using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ReliefManagementSystem.Domain.Entities;

[Index("CreatedBy", Name = "IX_InventoryTransactions_CreatedBy")]
[Index("InventoryId", Name = "IX_InventoryTransactions_InventoryId")]
public partial class InventoryTransaction
{
    [Key]
    public Guid TransactionId { get; set; }

    public Guid InventoryId { get; set; }

    public string TransactionCode { get; set; } = null!;

    public int Type { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid CreatedBy { get; set; }

    public string? Notes { get; set; }

    [ForeignKey("CreatedBy")]
    [InverseProperty("InventoryTransactions")]
    public virtual AspNetUser CreatedByNavigation { get; set; } = null!;

    [ForeignKey("InventoryId")]
    [InverseProperty("InventoryTransactions")]
    public virtual Inventory Inventory { get; set; } = null!;

    [InverseProperty("Transaction")]
    public virtual ICollection<InventoryTransactionItem> InventoryTransactionItems { get; set; } = new List<InventoryTransactionItem>();
}
