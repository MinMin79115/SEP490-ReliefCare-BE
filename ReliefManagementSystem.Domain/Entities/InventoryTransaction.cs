using ReliefManagementSystem.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Domain.Entities
{
   public class InventoryTransaction
    {
        public Guid TransactionId { get; set; }

        public Guid InventoryId { get; set; }

        public string TransactionCode { get; set; } = null!; // "IN-20260109-001", "OUT-20260109-002"

        public TransactionType Type { get; set; }

        public TransactionReason Reason { get; set; }


        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Guid CreatedBy { get; set; }
        public ApplicationUser CreatedByUser { get; set; } = null!;

        public string? Notes { get; set; }

        // Navigation properties - One transaction can have multiple items

        public Inventory Inventory { get; set; } = null!;

        public Guid? SupplyTransferId { get; set; }
        public SupplyTransfer? SupplyTransfer { get; set; }

        public string? ImportBatchCode { get; set; }
        public string? SourceReference { get; set; }

        public SupplyAllocation? SupplyAllocation { get; set; } // Liên kết 1-1 với phiếu cấp phát

        public ICollection<InventoryTransactionItem> Items { get; set; } = new List<InventoryTransactionItem>();
        public ICollection<InKindDonation> InKindDonations { get; set; } = new List<InKindDonation>();
    }
}
