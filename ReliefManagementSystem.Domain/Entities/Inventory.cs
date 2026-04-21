using ReliefManagementSystem.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Domain.Entities
{
    public class Inventory
    {
        public Guid InventoryId { get; set; }

        public Guid ReliefStationId { get; set; }

        public InventoryLevel Level { get; set; }

        public EntityStatus Status { get; set; }

        public ReliefStation ReliefStation { get; set; } = null!;
        public ICollection<InventoryStock> InventoryItems { get; set; } = new List<InventoryStock>();
        public ICollection<InventoryTransaction> InventoryTransactions { get; set; } = new List<InventoryTransaction>();
        public ICollection<ReliefPackageAssembly> ReliefPackageAssemblies { get; set; } = new List<ReliefPackageAssembly>();
    }
}
