using ReliefManagementSystem.Domain.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Domain.Entities
{
    public class InventoryStock
    {
        public Guid InventoryStockId { get; set; }

        public Guid InventoryId { get; set; }
        public Guid SupplyItemId { get; set; }

        public int CurrentQuantity { get; set; }

        public int MinimumStockLevel { get; set; }
        public int MaximumStockLevel { get; set; }

        public byte[] RowVersion { get; set; } = [];

        // Computed – KHÔNG map DB
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

        public Inventory Inventory { get; set; } = null!;
        public SupplyItem SupplyItem { get; set; } = null!;

    }
}
