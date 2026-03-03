using System;
using System.ComponentModel.DataAnnotations;

namespace ReliefManagementSystem.Domain.Entities
{
    /// <summary>
    /// Profile bổ sung cho user có role Moderator.
    /// Quan hệ 1:1 với ApplicationUser (giống ManagerProfile).
    /// </summary>
    public class ModeratorProfile
    {
        [Key]
        public Guid ModeratorProfileId { get; set; }

        // 1:1 với ApplicationUser
        public Guid UserId { get; set; }
        public ApplicationUser User { get; set; } = null!;

        /// <summary>
        /// Khu vực / phạm vi giám sát mà Moderator này được phân công.
        /// Ví dụ: "Miền Nam", "Tỉnh Bình Dương"…
        /// Null = chưa gán hoặc giám sát toàn quốc.
        /// </summary>
        public string? AssignedArea { get; set; }

        /// <summary>Ngày được bổ nhiệm làm Moderator.</summary>
        public DateTime AppointedAt { get; set; } = DateTime.UtcNow;

        /// <summary>Ghi chú bổ sung (phạm vi quyền, v.v.).</summary>
        public string? Notes { get; set; }
    }
}
