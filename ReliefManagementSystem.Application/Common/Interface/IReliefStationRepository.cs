using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Application.Common.Interface
{
    /// <summary>
    /// Repository interface for ReliefStation operations.
    /// </summary>
    public interface IReliefStationRepository : IGenericRepository<ReliefStation>
    {
        /// <summary>
        /// Gets all active (non-Closed) stations with Manager and Location info.
        /// </summary>
        Task<IReadOnlyList<ReliefStation>> GetAllWithDetailsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a single station with full details including teams and inventories.
        /// </summary>
        Task<ReliefStation?> GetByIdWithDetailsAsync(Guid stationId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets stations filtered by status.
        /// </summary>
        Task<IReadOnlyList<ReliefStation>> GetByStatusAsync(ReliefStationStatus status, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets stations managed by a specific user.
        /// </summary>
        Task<IReadOnlyList<ReliefStation>> GetByManagerIdAsync(Guid managerId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Checks whether a station with the same name already exists (case-insensitive).
        /// </summary>
        Task<bool> IsNameExistsAsync(string name, Guid? excludeId = null, CancellationToken cancellationToken = default);
    }
}
