using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Application.Features.RescueRequest.DTOs.Request
{
    /// <summary>DTO để gửi yêu cầu cứu hộ mới với attachments</summary>
    public class CreateRescueRequestDto
    {
        /// <summary>Loại yêu cầu: 0 = Normal (cần xác minh), 1 = Emergency (không cần xác minh)</summary>
        [Required(ErrorMessage = "Request type is required")]
        public RescueRequestType RescueType { get; set; }

        /// <summary>Loại thảm họa (Flood, Landslide, Earthquake, Fire, Storm, Other)</summary>
        [Required(ErrorMessage = "Disaster type is required")]
        public int DisasterType { get; set; }

        /// <summary>Mô tả chi tiết vụ cứu hộ</summary>
        [Required(ErrorMessage = "Description is required")]
        [MinLength(10, ErrorMessage = "Description must be at least 10 characters")]
        [MaxLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string Description { get; set; } = null!;

        /// <summary>Vĩ độ vị trí cần cứu hộ</summary>
        [Required(ErrorMessage = "Latitude is required")]
        [Range(-90, 90, ErrorMessage = "Latitude must be between -90 and 90")]
        public double Latitude { get; set; }

        /// <summary>Kinh độ vị trí cần cứu hộ</summary>
        [Required(ErrorMessage = "Longitude is required")]
        [Range(-180, 180, ErrorMessage = "Longitude must be between -180 and 180")]
        public double Longitude { get; set; }

        /// <summary>Độ chính xác vị trí (optional)</summary>
        [Range(0, double.MaxValue, ErrorMessage = "Accuracy must be a positive number")]
        public double? Accuracy { get; set; }

        /// <summary>Địa chỉ cụ thể</summary>
        [MaxLength(500, ErrorMessage = "Address cannot exceed 500 characters")]
        public string? Address { get; set; }

        /// <summary>ID địa điểm liên quan (tuỳ chọn)</summary>
        public Guid? LocationId { get; set; }

        /// <summary>Ghi chú bổ sung (tuỳ chọn)</summary>
        [MaxLength(500, ErrorMessage = "Note cannot exceed 500 characters")]
        public string? Note { get; set; }

        /// <summary>Thông tin người báo (nếu người dùng không đăng nhập)</summary>
        [MaxLength(200)]
        public string? ReporterFullName { get; set; }

        /// <summary>Số điện thoại người báo (nếu người dùng không đăng nhập)</summary>
        [MaxLength(50)]
        public string? ReporterPhone { get; set; }

        /// <summary>Danh sách file đính kèm (URLs hoặc file paths)</summary>
        public List<AttachmentDto>? Attachments { get; set; } = new();

        /// <summary>Danh sách PriorityCriteria Ids được user chọn (đọc từ seeder)</summary>
        public List<Guid>? SelectedPriorityCriteriaIds { get; set; } = new();

        /// <summary>DTO cho file đính kèm</summary>
        public class AttachmentDto
        {
            /// <summary>URL hoặc đường dẫn file</summary>
            [Required(ErrorMessage = "FileUrl is required")]
            public string FileUrl { get; set; } = null!;

            /// <summary>Loại content (image/jpeg, image/png, video/mp4, etc.)</summary>
            [Required(ErrorMessage = "ContentType is required")]
            public string ContentType { get; set; } = null!;
        }
    }
}