using ReliefManagementSystem.Domain.Entities;

namespace ReliefManagementSystem.Application.Common.Interface
{
    /// <summary>Interface for ManagerProfile data access.</summary>
    public interface IManagerProfileRepository : IGenericRepository<ManagerProfile>
    {
        /// <summary>
        /// Lấy ManagerProfile của user theo UserId (kèm AssignedLocation).
        /// Trả về null nếu user chưa có profile Manager.
        /// </summary>
        Task<ManagerProfile?> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    }
}
