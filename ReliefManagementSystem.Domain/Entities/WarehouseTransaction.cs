namespace ReliefManagementSystem.Domain.Entities
{
    /// <summary>
    /// Giao dịch nhập/xuất kho (chi tiết từng vật tư trong phiếu)
    /// </summary>
    public class WarehouseTransaction
    {
        public Guid TransactionId { get; set; }

        /// <summary>
        /// Phiếu nhập/xuất
        /// </summary>
        public Guid BatchId { get; set; }
        public ImportExportBatch Batch { get; set; } = null!;

        /// <summary>
        /// Vật tư
        /// </summary>
        public Guid InventoryItemId { get; set; }
        public InventoryItem InventoryItem { get; set; } = null!;

        /// <summary>
        /// Số lượng nhập/xuất
        /// </summary>
        public decimal Quantity { get; set; }

        /// <summary>
        /// Ghi chú riêng cho vật tư này
        /// </summary>
        public string? Notes { get; set; }
    }
}
