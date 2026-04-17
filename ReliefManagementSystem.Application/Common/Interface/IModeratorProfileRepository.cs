using ReliefManagementSystem.Domain.Entities;
using System.Collections.Generic;

namespace ReliefManagementSystem.Application.Common.Interface
{
    /// <summary>Interface for ModeratorProfile data access.</summary>
    public interface IModeratorProfileRepository : IGenericRepository<ModeratorProfile>
    {
        /// <summary>
        /// Lấy ModeratorProfile của user theo UserId (kèm ReliefStation).
        /// Trả về null nếu user chưa có profile Moderator.
        /// </summary>
        Task<ModeratorProfile?> GetByUserIdAsync(Guid userId, CancellationToken ct = default);

        /// <summary>
        /// Lấy trưởng trạm hiện tại của một trạm.
        /// </summary>
        Task<ModeratorProfile?> GetStationHeadAsync(Guid stationId, CancellationToken ct = default);

        /// <summary>
        /// Lấy danh sách moderator đang hoạt động theo trạm.
        /// </summary>
        Task<List<ModeratorProfile>> GetActiveByStationIdAsync(Guid stationId, CancellationToken ct = default);
    }
}
