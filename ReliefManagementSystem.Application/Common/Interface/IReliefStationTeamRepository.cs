using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Application.Common.Interface
{
    /// <summary>
    /// Repository interface for ReliefStationTeam (team assignments to a station).
    /// </summary>
    public interface IReliefStationTeamRepository : IGenericRepository<ReliefStationTeam>
    {
        /// <summary>
        /// Gets all team assignments for a station, including Team info.
        /// </summary>
        Task<IReadOnlyList<ReliefStationTeam>> GetByStationIdAsync(Guid stationId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a specific assignment by station + team combination.
        /// </summary>
        Task<ReliefStationTeam?> GetByStationAndTeamAsync(Guid stationId, Guid teamId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Checks if a team is already assigned to a station (regardless of status).
        /// </summary>
        Task<bool> IsTeamAssignedAsync(Guid stationId, Guid teamId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a specific assignment by ID with Team info.
        /// </summary>
        Task<ReliefStationTeam?> GetByIdWithDetailsAsync(Guid assignmentId, CancellationToken cancellationToken = default);

        IQueryable<ReliefStationTeam> GetQueryableWithTeamDetails();
    }
}
