namespace ReliefManagementSystem.Domain.Enum
{
    /// <summary>
    /// Trạng thái tồn kho của vật tư
    /// </summary>
    public enum InventoryStatus
    {
        /// <summary>
        /// Nguy cấp - Dưới 15% capacity
        /// </summary>
        Critical,

        /// <summary>
        /// Cần bổ sung - 15-49% capacity
        /// </summary>
        Low,

        /// <summary>
        /// An toàn - 50-99% capacity
        /// </summary>
        Safe,

        /// <summary>
        /// Đầy kho - 100% capacity
        /// </summary>
        Full
    }
}
