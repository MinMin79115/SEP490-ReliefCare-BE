using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Domain.Entities
{
    public class InventoryTransactionItem
    {
        public Guid TransactionItemId { get; set; }

        public Guid TransactionId { get; set; }

        public Guid SupplyItemId { get; set; }

        public int Quantity { get; set; }

        public string? Notes { get; set; }

        // Navigation
        public InventoryTransaction Transaction { get; set; } = null!;
        public SupplyItem SupplyItem { get; set; } = null!;
    }
}
