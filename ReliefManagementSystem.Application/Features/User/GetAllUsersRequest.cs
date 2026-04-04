namespace ReliefManagementSystem.Application.Features.User
{
    /// <summary>
    /// Request phân trang danh sách users.
    /// Frontend gửi qua query string: ?pageIndex=1&pageSize=10
    /// </summary>
    public class GetAllUsersRequest
    {
        /// <summary>
        /// Trang hiện tại (mặc định = 1)
        /// </summary>
        public int PageIndex { get; set; } = 1;

        /// <summary>
        /// Số lượng items mỗi trang (mặc định = 10)
        /// </summary>
        public int PageSize { get; set; } = 10;

        /// <summary>
        /// Từ khóa tìm kiếm (DisplayName, Email, PhoneNumber)
        /// </summary>
        public string? Search { get; set; }

        /// <summary>
        /// Lọc theo role, ví dụ: Admin, Moderator, Volunteer, User
        /// </summary>
        public string? Role { get; set; }

        /// <summary>
        /// Lọc trạng thái ban: true = bị ban, false = không bị ban
        /// </summary>
        public bool? IsBanned { get; set; }
    }
}
