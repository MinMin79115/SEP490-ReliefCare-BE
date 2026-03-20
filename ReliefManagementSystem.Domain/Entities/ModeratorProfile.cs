using System;
using System.ComponentModel.DataAnnotations;
using ReliefManagementSystem.Domain.Enum;

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

        public Guid? ReliefStationId { get; set; }
        public ReliefStation? ReliefStation { get; set; }

        public bool IsStationHead { get; set; } = false;

        public DateTime AppointedAt { get; set; } = DateTime.UtcNow;

        public string? Notes { get; set; }

        public ModeratorStatus Status { get; set; } = ModeratorStatus.Inactive;

        [MaxLength(500)]
        public string? StatusReason { get; set; }
    }
}
