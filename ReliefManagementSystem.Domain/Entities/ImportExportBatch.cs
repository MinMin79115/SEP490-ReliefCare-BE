using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Domain.Entities
{
    /// <summary>
    /// Phiếu nhập/xuất kho (batch transaction)
    /// </summary>
    public class ImportExportBatch
    {
        public Guid BatchId { get; set; }

        /// <summary>
        /// Mã phiếu (PN-2026-001, PX-2026-001)
        /// </summary>
        public string BatchNumber { get; set; } = null!;

        /// <summary>
        /// Loại phiếu (Nhập/Xuất)
        /// </summary>
        public TransactionType BatchType { get; set; }

        /// <summary>
        /// Người tạo phiếu
        /// </summary>
        public Guid CreatedBy { get; set; }
        public ApplicationUser Creator { get; set; } = null!;

        /// <summary>
        /// Thời gian tạo
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Ghi chú chung cho phiếu
        /// </summary>
        public string? Notes { get; set; }

        /// <summary>
        /// Thông tin người nhận (dùng cho xuất kho)
        /// </summary>
        public string? RecipientInfo { get; set; }

        /// <summary>
        /// Trạng thái phiếu
        /// </summary>
        public BatchStatus Status { get; set; } = BatchStatus.Confirmed;

        // Navigation properties
        public ICollection<WarehouseTransaction> Transactions { get; set; } = new List<WarehouseTransaction>();
    }
}
