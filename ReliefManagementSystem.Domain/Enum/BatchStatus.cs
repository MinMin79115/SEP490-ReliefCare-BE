namespace ReliefManagementSystem.Domain.Enum
{
    /// <summary>
    /// Trạng thái phiếu nhập/xuất kho
    /// </summary>
    public enum BatchStatus
    {
        /// <summary>
        /// Nháp - chưa xác nhận
        /// </summary>
        Draft,

        /// <summary>
        /// Đã xác nhận
        /// </summary>
        Confirmed,

        /// <summary>
        /// Đã hủy
        /// </summary>
        Cancelled
    }
}
