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

        /// <summary>Optional URL of the icon image for this supply item.</summary>
        public string? IconUrl { get; set; }

        public SupplyCategory Category { get; set; }

        public string Unit { get; set; } = null!; 

        public decimal? EstimatedUnitCost { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public ICollection<InventoryStock> InventoryItems { get; set; } = new List<InventoryStock>();
        public ICollection<CampaignInventoryStock> CampaignInventoryStocks { get; set; } = new List<CampaignInventoryStock>();
        public ICollection<InventoryTransactionItem> InventoryTransactionItems { get; set; } = new List<InventoryTransactionItem>();
        public ICollection<CampaignInventoryTransactionItem> CampaignInventoryTransactionItems { get; set; } = new List<CampaignInventoryTransactionItem>();
        public ICollection<SupplyAllocationItem> SupplyAllocationItems { get; set; } = new List<SupplyAllocationItem>();
        public ICollection<ReliefPackageDefinition> OutputOfReliefPackageDefinitions { get; set; } = new List<ReliefPackageDefinition>();
        public ICollection<ReliefPackageAssembly> OutputReliefPackageAssemblies { get; set; } = new List<ReliefPackageAssembly>();
        public ICollection<ReliefPackageAssemblyDetail> ReliefPackageAssemblyDetails { get; set; } = new List<ReliefPackageAssemblyDetail>();

        public ICollection<InKindDonationDetail> InKindDonationDetails { get; set; } = new List<InKindDonationDetail>();

    }
}
