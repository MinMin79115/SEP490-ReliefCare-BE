namespace ReliefManagementSystem.Domain.Entities
{
    public class ReliefPackageDefinition
    {
        public Guid ReliefPackageDefinitionId { get; set; }

        public Guid CampaignId { get; set; }
        public Guid OutputSupplyItemId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal? CashSupportAmount { get; set; }
        public bool IsDefault { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Campaign Campaign { get; set; } = null!;
        public SupplyItem OutputSupplyItem { get; set; } = null!;
        public ICollection<ReliefPackageDefinitionItem> Items { get; set; } = new List<ReliefPackageDefinitionItem>();
        public ICollection<HouseholdDelivery> HouseholdDeliveries { get; set; } = new List<HouseholdDelivery>();
        public ICollection<ReliefPackageAssembly> PackageAssemblies { get; set; } = new List<ReliefPackageAssembly>();
    }
}
