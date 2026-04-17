using ReliefManagementSystem.Domain.Enum;
using System;
using System.ComponentModel.DataAnnotations;

namespace ReliefManagementSystem.Domain.Entities
{
    /// <summary>
    /// Lưu thông báo gửi đến người dùng.
    /// Kết hợp với tầng realtime publisher để push real-time; đồng thời persist DB
    /// để user offline vẫn nhận được khi kết nối lại.
    /// </summary>
    public class Notification 
    {
        public Guid NotificationId { get; set; }

        // ── Người nhận ────────────────────────────────────────────
        /// <summary>FK → ApplicationUser (người nhận thông báo)</summary>
        public Guid RecipientId { get; set; }

        // ── Nội dung ──────────────────────────────────────────────
        public NotificationType Type { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = null!;

        [MaxLength(1000)]
        public string? Message { get; set; }

        // ── Deep-link đến entity liên quan ────────────────────────
        /// <summary>
        /// ID của entity liên quan (RequestId, SupplyTransferId…).
        /// Dùng để deep-link từ thông báo vào màn hình tương ứng.
        /// </summary>
        public Guid? ReferenceId { get; set; }

        /// <summary>
        /// Tên loại entity: "RescueRequest", "SupplyTransfer"…
        /// </summary>
        [MaxLength(100)]
        public string? ReferenceType { get; set; }

        [MaxLength(4000)]
        public string? MetadataJson { get; set; }

        // ── Trạng thái đọc ────────────────────────────────────────
        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>Thời điểm user đánh dấu đã đọc</summary>
        public DateTime? ReadAt { get; set; }

        // ── Navigation ────────────────────────────────────────────
        public ApplicationUser Recipient { get; set; } = null!;
    }
}
