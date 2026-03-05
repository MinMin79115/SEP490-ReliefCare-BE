using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Application.Features.ReliefStation.DTOs.Request
{
    /// <summary>
    /// Request phân trang và lọc danh sách trạm cứu trợ.
    /// Frontend gửi qua query string: ?pageIndex=1&amp;pageSize=10&amp;level=1&amp;search=Bình
    /// </summary>
    public class GetAllStationsRequest
    {
        /// <summary>Trang hiện tại (mặc định = 1).</summary>
        public int PageIndex { get; set; } = 1;

        /// <summary>Số lượng trạm mỗi trang (mặc định = 10).</summary>
        public int PageSize { get; set; } = 10;

        /// <summary>
        /// Lọc theo cấp trạm (tuỳ chọn).
        /// <br/>1 = Regional (vùng) | 2 = Provincial (tỉnh) | 3 = Local (địa phương).
        /// <br/>Nếu không truyền → trả về tất cả các cấp.
        /// </summary>
        public ReliefStationLevel? Level { get; set; }

        /// <summary>
        /// Tìm kiếm theo tên trạm (tuỳ chọn, tìm kiếm chứa — contains).
        /// </summary>
        public string? Search { get; set; }
    }
}
