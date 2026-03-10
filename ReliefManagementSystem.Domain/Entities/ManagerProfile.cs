using ReliefManagementSystem.Domain.Enum;
using System;
using System.ComponentModel.DataAnnotations;

namespace ReliefManagementSystem.Domain.Entities
{
    public class ManagerProfile
    {
        [Key]
        public Guid ManagerProfileId { get; set; }

        // 1:1 với ApplicationUser
        public Guid UserId { get; set; }
        public ApplicationUser User { get; set; } = null!;

        /// <summary>
        /// Cấp quản lý: Region (vùng) | Province (tỉnh) | Commune (địa phương).
        /// </summary>
        public LocationLevel Level { get; set; }

        /// <summary>
        /// Địa phương/vùng/tỉnh mà manager này phụ trách.
        /// Null = chưa gán hoặc phụ trách toàn quốc.
        /// </summary>
        public Guid? AssignedLocationId { get; set; }
        public Location? AssignedLocation { get; set; }

        /// <summary>
        /// Ngày được bổ nhiệm.
        /// </summary>
        public DateTime AppointedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Ghi chú bổ sung (phạm vi phụ trách, quyền đặc biệt, v.v.).
        /// </summary>
        public string? Notes { get; set; }
    }
}
