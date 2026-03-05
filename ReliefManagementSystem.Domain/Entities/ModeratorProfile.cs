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
        /// Trạm cứu trợ mà Moderator này được phân công.
        /// Null = chưa gán vào trạm nào.
        /// </summary>
        public Guid? ReliefStationId { get; set; }
        public ReliefStation? ReliefStation { get; set; }

        /// <summary>
        /// True nếu Moderator này là người đứng đầu trạm (trưởng trạm).
        /// Mỗi trạm chỉ có tối đa 1 Moderator có IsStationHead = true.
        /// </summary>
        public bool IsStationHead { get; set; } = false;

        /// <summary>Ngày được bổ nhiệm làm Moderator.</summary>
        public DateTime AppointedAt { get; set; } = DateTime.UtcNow;

        /// <summary>Ghi chú bổ sung (phạm vi quyền, v.v.).</summary>
        public string? Notes { get; set; }
    }
}
