namespace ReliefManagementSystem.Application.Features.RescueRequest.DTOs.Request
{
    /// <summary>DTO dùng để lọc danh sách yêu cầu cứu hộ của người dùng hiện tại</summary>
    public class MyRescueRequestQueryDto
    {
        /// <summary>Trang hiện tại (bắt đầu từ 1)</summary>
        public int PageNumber { get; set; } = 1;

        /// <summary>Số lượng bản ghi mỗi trang</summary>
        public int PageSize { get; set; } = 10;

        /// <summary>Lọc theo trạng thái (null = tất cả). Xem enum RescueRequestStatus để biết các giá trị hợp lệ.</summary>
        public int? StatusFilter { get; set; }
    }
}
