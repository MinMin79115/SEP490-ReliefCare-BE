namespace ReliefManagementSystem.Application.Features.RescueRequest.DTOs.Request
{
    /// <summary>DTO dùng khi người dân tự hủy yêu cầu cứu hộ đã gửi</summary>
    public class CancelRescueRequestDto
    {
        /// <summary>Lý do hủy yêu cầu (bắt buộc)</summary>
        public string Reason { get; set; } = string.Empty;
    }
}
