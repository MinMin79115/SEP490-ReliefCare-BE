using ReliefManagementSystem.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Domain.Entities
{
    public class SupplyItem
    {
        public Guid SupplyItemId { get; set; }

        public string Name { get; set; } = null!;

        public string? Description { get; set; }

        public SupplyCategory Category { get; set; }

        public string Unit { get; set; } = null!; // "Thùng", "Cái", "Hộp", "Bộ"

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public ICollection<InventoryStock> InventoryItems { get; set; } = new List<InventoryStock>();
        public ICollection<InventoryTransactionItem> InventoryTransactionItems { get; set; } = new List<InventoryTransactionItem>();
    }
}
