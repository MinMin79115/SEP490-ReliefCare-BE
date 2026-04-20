namespace ReliefManagementSystem.Application.Features.RescueRequest.DTOs.Response
{
    // ─── Stats DTO ───────────────────────────────────────────────────────────────

    /// <summary>DTO thống kê số lượng rescue request theo từng trạng thái — dùng cho Admin/Moderator dashboard</summary>
    public class RescueRequestStatsDto
    {
        /// <summary>Tổng số yêu cầu trong hệ thống</summary>
        public int Total { get; set; }

        /// <summary>Đang chờ xét duyệt</summary>
        public int Pending { get; set; }

        /// <summary>Đã xác minh, chờ gán team</summary>
        public int Verified { get; set; }

        /// <summary>Đã gán team, chờ team tiếp nhận</summary>
        public int Assigned { get; set; }

        /// <summary>Đang được team xử lý ngoài hiện trường</summary>
        public int InProgress { get; set; }

        /// <summary>Đã cứu hộ hoàn thành</summary>
        public int Completed { get; set; }

        /// <summary>Đã hủy (do người dân hủy hoặc bị từ chối)</summary>
        public int Cancelled { get; set; }
    }

    // ─── Team Location DTO ───────────────────────────────────────────────────────

    /// <summary>
    /// DTO vị trí realtime của đội cứu hộ đang xử lý yêu cầu.
    /// Public — không yêu cầu đăng nhập, truy cập qua RequestId.
    /// Không chứa thông tin nhạy cảm của team member.
    /// </summary>
    public class TeamLocationForRequestDto
    {
        /// <summary>ID của rescue operation đang Active</summary>
        public Guid RescueOperationId { get; set; }

        /// <summary>ID của team đang được gán</summary>
        public Guid TeamId { get; set; }

        /// <summary>Tên team</summary>
        public string TeamName { get; set; } = null!;

        /// <summary>Trạng thái hiện tại của operation (EnRoute, Rescuing, Returning...)</summary>
        public string OperationStatus { get; set; } = null!;

        /// <summary>Vĩ độ mới nhất của team (null nếu chưa có heartbeat)</summary>
        public double? CurrentLatitude { get; set; }

        /// <summary>Kinh độ mới nhất của team</summary>
        public double? CurrentLongitude { get; set; }

        /// <summary>Thời điểm ghi nhận vị trí mới nhất</summary>
        public DateTime? LastTrackedAt { get; set; }

        /// <summary>ETA ước tính đến điểm nạn nhân (phút) — từ RescueBatchItem, null nếu chưa tính</summary>
        public int? EstimatedMinutesToArrival { get; set; }

        /// <summary>Khoảng cách còn lại đến điểm nạn nhân (km)</summary>
        public double? DistanceKmToVictim { get; set; }
    }

    // ─── Team History DTO ────────────────────────────────────────────────────────

    /// <summary>DTO lịch sử các ca cứu hộ đã hoàn thành của một team (phân trang)</summary>
    public class RescueTeamHistoryResponseDto
    {
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public List<RescueBatchHistoryItemDto> Data { get; set; } = new();
    }

    /// <summary>Tóm tắt một batch (ca trực) đã hoàn thành của team</summary>
    public class RescueBatchHistoryItemDto
    {
        public Guid RescueBatchId { get; set; }

        /// <summary>Thời điểm bắt đầu batch</summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>Thời điểm đóng batch</summary>
        public DateTime? ClosedAt { get; set; }

        /// <summary>Tổng số request trong batch</summary>
        public int TotalRequests { get; set; }

        /// <summary>Số request đã hoàn thành (Done)</summary>
        public int CompletedRequests { get; set; }

        /// <summary>Chi tiết từng request trong batch</summary>
        public List<RescueCompletedRequestSummaryDto> Requests { get; set; } = new();
    }

    /// <summary>Tóm tắt 1 rescue request đã hoàn thành trong batch lịch sử</summary>
    public class RescueCompletedRequestSummaryDto
    {
        public Guid RequestId { get; set; }
        public Guid? VehicleId { get; set; }
        public string? VehicleName { get; set; }
        public string? VehicleLicensePlate { get; set; }
        public string? Address { get; set; }
        public string DisasterType { get; set; } = null!;

        /// <summary>Loại cứu hộ: Emergency / Normal</summary>
        public string? RescueRequestType { get; set; }

        /// <summary>Điểm ưu tiên tổng hợp</summary>
        public int? Priority { get; set; }

        /// <summary>Cấp ưu tiên: Low / Medium / High / Critical</summary>
        public string? PriorityLevel { get; set; }

        public string RescueRequestStatus { get; set; } = null!;
        public string ReporterFullName { get; set; } = null!;
        public string ReporterPhone { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        /// <summary>Thứ tự xử lý trong batch</summary>
        public int SequenceOrder { get; set; }

        /// <summary>Trạng thái của item trong batch (Done/Cancelled...)</summary>
        public string BatchItemStatus { get; set; } = null!;
    }
}
