namespace ReliefManagementSystem.Domain.Entities
{
    public class ReliefPackageDefinitionItem
    {
        public Guid ReliefPackageDefinitionItemId { get; set; }

        public Guid ReliefPackageDefinitionId { get; set; }
        public Guid SupplyItemId { get; set; }
        public int Quantity { get; set; }
        public string Unit { get; set; } = string.Empty;

        public ReliefPackageDefinition ReliefPackageDefinition { get; set; } = null!;
        public SupplyItem SupplyItem { get; set; } = null!;
    }
}
