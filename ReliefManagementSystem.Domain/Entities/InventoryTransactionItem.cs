using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ReliefManagementSystem.Domain.Entities;

[Index("SupplyItemId", Name = "IX_InventoryTransactionItems_SupplyItemId")]
[Index("TransactionId", Name = "IX_InventoryTransactionItems_TransactionId")]
public partial class InventoryTransactionItem
{
    [Key]
    public Guid TransactionItemId { get; set; }

    public Guid TransactionId { get; set; }

    public Guid SupplyItemId { get; set; }

    public int Quantity { get; set; }

    public string? Notes { get; set; }

    [ForeignKey("SupplyItemId")]
    [InverseProperty("InventoryTransactionItems")]
    public virtual SupplyItem SupplyItem { get; set; } = null!;

    [ForeignKey("TransactionId")]
    [InverseProperty("InventoryTransactionItems")]
    public virtual InventoryTransaction Transaction { get; set; } = null!;
}
