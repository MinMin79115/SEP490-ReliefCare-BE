using ReliefManagementSystem.Domain.Enum;
using System.ComponentModel.DataAnnotations;

namespace ReliefManagementSystem.Application.Features.ReliefStation.DTOs.Request
{
    /// <summary>Request model to create a new relief station.</summary>
    public class CreateReliefStationRequest
    {
        [Required(ErrorMessage = "Name is required.")]
        [MaxLength(255)]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "LocationId is required.")]
        public Guid LocationId { get; set; }

        [MaxLength(500)]
        public string? Address { get; set; }

        [MaxLength(20)]
        public string? ContactNumber { get; set; }

        public double Longitude { get; set; }
        public double Latitude { get; set; }

        public ReliefStationStatus Status { get; set; } = ReliefStationStatus.Draft;
    }

    /// <summary>
    /// Request model để <b>Manager</b> tạo trạm cứu trợ cấp Tỉnh (Provincial).
    /// <br/><br/>
    /// <b>Lưu ý cho Frontend:</b><br/>
    /// - <c>LocationId</c> phải là ID của một địa điểm cấp <b>Tỉnh</b> (LocationLevel = Province = 2).<br/>
    /// - Hệ thống tự động tìm trạm Regional cha dựa trên vùng mà Manager phụ trách.<br/>
    /// - Nếu LocationId không thuộc vùng phụ trách → lỗi 404 PARENT_STATION_NOT_FOUND.<br/>
    /// - Nếu LocationId không phải cấp Tỉnh → lỗi 400 INVALID_LOCATION_LEVEL.
    /// </summary>
    public class CreateProvincialStationRequest
    {
        /// <summary>
        /// Tên trạm cứu trợ. Bắt buộc, tối đa 255 ký tự.
        /// </summary>
        [Required(ErrorMessage = "Tên trạm là bắt buộc.")]
        [MaxLength(255, ErrorMessage = "Tên trạm không được vượt quá 255 ký tự.")]
        public string Name { get; set; } = null!;

        /// <summary>
        /// ID địa điểm cấp <b>Tỉnh</b> mà trạm này phục vụ.
        /// Dùng API GET /api/locations?level=2 để lấy danh sách LocationId cấp Tỉnh.
        /// </summary>
        [Required(ErrorMessage = "LocationId là bắt buộc.")]
        public Guid LocationId { get; set; }

        /// <summary>Địa chỉ cụ thể của trạm (tuỳ chọn).</summary>
        [MaxLength(500)]
        public string? Address { get; set; }

        /// <summary>Số điện thoại liên hệ của trạm (tuỳ chọn).</summary>
        [MaxLength(20)]
        public string? ContactNumber { get; set; }

        /// <summary>Kinh độ (longitude) vị trí trạm.</summary>
        public double Longitude { get; set; }

        /// <summary>Vĩ độ (latitude) vị trí trạm.</summary>
        public double Latitude { get; set; }
    }

    /// <summary>
    /// Request model để <b>Moderator (trưởng trạm tỉnh)</b> tạo trạm cứu trợ cấp Địa phương (Local).
    /// <br/><br/>
    /// <b>Lưu ý cho Frontend:</b><br/>
    /// - Chỉ Moderator có <c>IsStationHead = true</c> tại một trạm Provincial mới được gọi API này.<br/>
    /// - <c>LocationId</c> phải là ID của một địa điểm cấp <b>Xã/Phường</b> (LocationLevel = Commune = 3).<br/>
    /// - Hệ thống tự động gán <c>parentReliefStationId</c> = trạm tỉnh mà Moderator đang đứng đầu.<br/>
    /// - Nếu LocationId không phải cấp Xã → lỗi 400 INVALID_LOCATION_LEVEL.<br/>
    /// - Nếu user không phải trưởng trạm → lỗi 403 UNAUTHORIZED_STATION_CREATION.
    /// </summary>
    public class CreateLocalStationRequest
    {
        /// <summary>
        /// Tên trạm cứu trợ. Bắt buộc, tối đa 255 ký tự.
        /// </summary>
        [Required(ErrorMessage = "Tên trạm là bắt buộc.")]
        [MaxLength(255, ErrorMessage = "Tên trạm không được vượt quá 255 ký tự.")]
        public string Name { get; set; } = null!;

        /// <summary>
        /// ID địa điểm cấp <b>Xã/Phường</b> mà trạm này phục vụ.
        /// Dùng API GET /api/locations?level=3 để lấy danh sách LocationId cấp Xã.
        /// </summary>
        [Required(ErrorMessage = "LocationId là bắt buộc.")]
        public Guid LocationId { get; set; }

        /// <summary>Địa chỉ cụ thể của trạm (tuỳ chọn).</summary>
        [MaxLength(500)]
        public string? Address { get; set; }

        /// <summary>Số điện thoại liên hệ của trạm (tuỳ chọn).</summary>
        [MaxLength(20)]
        public string? ContactNumber { get; set; }

        /// <summary>Kinh độ (longitude) vị trí trạm.</summary>
        public double Longitude { get; set; }

        /// <summary>Vĩ độ (latitude) vị trí trạm.</summary>
        public double Latitude { get; set; }
    }
    /// <summary>Request model to update an existing relief station.</summary>
    public class UpdateReliefStationRequest
    {
        [Required(ErrorMessage = "Name is required.")]
        [MaxLength(255)]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "LocationId is required.")]
        public Guid LocationId { get; set; }

        [Required(ErrorMessage = "ManagerId is required.")]
        public Guid ManagerId { get; set; }

        [MaxLength(500)]
        public string? Address { get; set; }

        [MaxLength(20)]
        public string? ContactNumber { get; set; }

        public double Longitude { get; set; }
        public double Latitude { get; set; }

        public ReliefStationStatus Status { get; set; }
    }

    /// <summary>Request model to assign a team to a relief station.</summary>
    public class AssignTeamRequest
    {
        [Required(ErrorMessage = "TeamId is required.")]
        public Guid TeamId { get; set; }
    }

    /// <summary>Request model to update the assignment status of a team at a station.</summary>
    public class UpdateTeamAssignmentRequest
    {
        [Required(ErrorMessage = "Status is required.")]
        public ReliefTeamAssignmentStatus Status { get; set; }

        /// <summary>Request model để gán Moderator vào một trạm cứu trợ.</summary>
        public class AssignModeratorRequest
        {
            /// <summary>UserId của Moderator cần thao tác.</summary>
            [Required(ErrorMessage = "ModeratorUserId là bắt buộc.")]
            public Guid ModeratorUserId { get; set; }

            /// <summary>Có gán làm trưởng trạm hay không? (1 trạm chỉ có 1 trưởng trạm).</summary>
            public bool IsStationHead { get; set; } = false;

            /// <summary>
            /// Trạng thái mới của Moderator (tuỳ chọn).
            /// Nếu null, hệ thống tự động gán là Active.
            /// </summary>
            public ModeratorStatus? Status { get; set; }

            /// <summary>Lý do thay đổi trạng thái hoặc lý do phân công (tuỳ chọn).</summary>
            [MaxLength(500)]
            public string? Reason { get; set; }
        }
    }
}
