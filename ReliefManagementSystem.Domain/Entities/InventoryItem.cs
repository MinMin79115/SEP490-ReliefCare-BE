using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Domain.Entities
{
    /// <summary>
    /// Vật tư trong kho
    /// </summary>
    public class InventoryItem
    {
        public Guid InventoryItemId { get; set; }

        /// <summary>
        /// Danh mục
        /// </summary>
        public Guid CategoryId { get; set; }
        public Category Category { get; set; } = null!;

        /// <summary>
        /// Mã vật tư (MI-TOM-001)
        /// </summary>
        public string Code { get; set; } = null!;

        /// <summary>
        /// Tên vật tư
        /// </summary>
        public string Name { get; set; } = null!;

        /// <summary>
        /// Mô tả
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Đơn vị tính (Thùng, Hộp, Cái, Kg...)
        /// </summary>
        public string Unit { get; set; } = null!;

        /// <summary>
        /// Số lượng hiện tại
        /// </summary>
        public decimal CurrentQuantity { get; set; }

        /// <summary>
        /// Sức chứa tối đa
        /// </summary>
        public decimal MaxCapacity { get; set; }

        /// <summary>
        /// Ngưỡng cảnh báo tối thiểu (15% của MaxCapacity)
        /// </summary>
        public decimal MinThreshold { get; set; }

        /// <summary>
        /// Trạng thái tồn kho
        /// </summary>
        public InventoryStatus Status { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public ICollection<WarehouseTransaction> Transactions { get; set; } = new List<WarehouseTransaction>();
    }
}
