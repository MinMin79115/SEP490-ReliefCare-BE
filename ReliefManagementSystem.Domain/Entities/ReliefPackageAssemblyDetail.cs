namespace ReliefManagementSystem.Domain.Entities
{
    public class ReliefPackageAssemblyDetail
    {
        public Guid ReliefPackageAssemblyDetailId { get; set; }

        public Guid ReliefPackageAssemblyId { get; set; }
        public Guid SupplyItemId { get; set; }
        public int QuantityConsumed { get; set; }
        public string Unit { get; set; } = string.Empty;

        public ReliefPackageAssembly ReliefPackageAssembly { get; set; } = null!;
        public SupplyItem SupplyItem { get; set; } = null!;
    }
}
