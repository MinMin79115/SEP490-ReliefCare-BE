using ReliefManagementSystem.Domain.Enum;
using System;
using System.Collections.Generic;

namespace ReliefManagementSystem.Domain.Entities
{
    public class CampaignInventoryTransaction
    {
        public Guid CampaignInventoryTransactionId { get; set; }
        public Guid CampaignInventoryId { get; set; }
        public string TransactionCode { get; set; } = null!;
        public TransactionType Type { get; set; }
        public TransactionReason Reason { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public Guid CreatedBy { get; set; }
        public string? Notes { get; set; }
        public Guid? SupplyAllocationId { get; set; }
        public Guid? CampaignTeamId { get; set; }
        public Guid? DistributionPointId { get; set; }
        public Guid? HouseholdDeliveryId { get; set; }
        public Guid? ReliefPackageDefinitionId { get; set; }

        public CampaignInventory CampaignInventory { get; set; } = default!;
        public ApplicationUser CreatedByUser { get; set; } = null!;
        public SupplyAllocation? SupplyAllocation { get; set; }
        public ICollection<CampaignInventoryTransactionItem> Items { get; set; } = new List<CampaignInventoryTransactionItem>();
    }
}
