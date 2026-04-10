using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Application.Features.Inventory.DTOs.Response
{
    /// <summary>
    /// Response for a single stock entry inside an inventory.
    /// </summary>
    public class InventoryStockResponse
    {
        public Guid InventoryStockId { get; set; }
        public Guid InventoryId { get; set; }

        // Supply Item info
        public Guid SupplyItemId { get; set; }
        public string SupplyItemName { get; set; } = null!;
        public string SupplyItemUnit { get; set; } = null!;
        public SupplyCategory SupplyItemCategory { get; set; }
        public string SupplyItemCategoryName => SupplyItemCategory.ToString();

        // Stock levels
        public int CurrentQuantity { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public int MinimumStockLevel { get; set; }
        public int MaximumStockLevel { get; set; }

        // Computed status (mirrors domain logic, no DB column)
        public InventoryStatus StockStatus { get; set; }
        public string StockStatusName => StockStatus.ToString();

        /// <summary>Percentage of capacity used (0-100+).</summary>
        public decimal FillPercentage => MaximumStockLevel > 0
            ? Math.Round((decimal)CurrentQuantity / MaximumStockLevel * 100, 1)
            : 0;
    }
}
