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
    }
}
