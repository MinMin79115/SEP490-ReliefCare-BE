namespace ReliefManagementSystem.Application.Features.Inventory
{
    public class DashboardStatsDto
    {
        /// <summary>
        /// Tổng số danh mục vật tư
        /// </summary>
        public int TotalCategories { get; set; }

        /// <summary>
        /// Số vật tư mới hôm nay
        /// </summary>
        public int NewItemsToday { get; set; }

        /// <summary>
        /// Số vật tư sắp hết hàng (Critical + Low)
        /// </summary>
        public int LowStockItems { get; set; }

        /// <summary>
        /// Tổng số lượng đã xuất hôm nay
        /// </summary>
        public decimal TotalExportedToday { get; set; }

        /// <summary>
        /// Phần trăm sức chứa kho trung bình
        /// </summary>
        public decimal AverageCapacityUsage { get; set; }

        /// <summary>
        /// Danh sách vật tư cảnh báo
        /// </summary>
        public List<InventoryItemDto> CriticalItems { get; set; } = new();
    }
}
