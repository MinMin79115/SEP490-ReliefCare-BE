namespace ReliefManagementSystem.Domain.Entities
{
    public class ReliefPackageAssembly
    {
        public Guid ReliefPackageAssemblyId { get; set; }

        public Guid CampaignId { get; set; }
        public Guid ReliefStationId { get; set; }
        public Guid InventoryId { get; set; }
        public Guid ReliefPackageDefinitionId { get; set; }
        public Guid OutputSupplyItemId { get; set; }

        public int QuantityCreated { get; set; }

        public Guid CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? Notes { get; set; }

        public Campaign Campaign { get; set; } = null!;
        public ReliefStation ReliefStation { get; set; } = null!;
        public Inventory Inventory { get; set; } = null!;
        public ReliefPackageDefinition ReliefPackageDefinition { get; set; } = null!;
        public SupplyItem OutputSupplyItem { get; set; } = null!;
        public ApplicationUser CreatedByUser { get; set; } = null!;

        public ICollection<ReliefPackageAssemblyDetail> Details { get; set; } = new List<ReliefPackageAssemblyDetail>();
    }
}
