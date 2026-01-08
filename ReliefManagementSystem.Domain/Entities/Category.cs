namespace ReliefManagementSystem.Domain.Entities
{
    /// <summary>
    /// Danh mục vật tư (Lương thực, Y tế, Dụng cụ...)
    /// </summary>
    public class Category
    {
        public Guid CategoryId { get; set; }

        /// <summary>
        /// Mã danh mục (FOOD, MEDICAL, TOOLS...)
        /// </summary>
        public string Code { get; set; } = null!;

        /// <summary>
        /// Tên danh mục
        /// </summary>
        public string Name { get; set; } = null!;

        /// <summary>
        /// Mô tả
        /// </summary>
        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public ICollection<InventoryItem> InventoryItems { get; set; } = new List<InventoryItem>();
    }
}
