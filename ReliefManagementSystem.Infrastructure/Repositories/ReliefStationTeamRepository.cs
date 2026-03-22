using Microsoft.EntityFrameworkCore;
using ReliefManagementSystem.Application.Common.Interface;
using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Infrastructure.Data;

namespace ReliefManagementSystem.Infrastructure.Repositories
{
    /// <summary>
    /// Concrete implementation of <see cref="IReliefStationTeamRepository"/>.
    /// </summary>
    public class ReliefStationTeamRepository : GenericRepository<ReliefStationTeam>, IReliefStationTeamRepository
    {
        public ReliefStationTeamRepository(ApplicationDbContext context) : base(context)
        {
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<ReliefStationTeam>> GetByStationIdAsync(
            Guid stationId,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(rst => rst.ReliefStationId == stationId)
                .Include(rst => rst.Team)
                .OrderBy(rst => rst.Status)
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<ReliefStationTeam?> GetByStationAndTeamAsync(
            Guid stationId,
            Guid teamId,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(rst => rst.Team)
                .FirstOrDefaultAsync(
                    rst => rst.ReliefStationId == stationId && rst.TeamId == teamId,
                    cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<bool> IsTeamAssignedAsync(
            Guid stationId,
            Guid teamId,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet.AnyAsync(
                rst => rst.ReliefStationId == stationId && rst.TeamId == teamId,
                cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<ReliefStationTeam?> GetByIdWithDetailsAsync(
            Guid assignmentId,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(rst => rst.Team)
                .Include(rst => rst.ReliefStation)
                .FirstOrDefaultAsync(rst => rst.ReliefStationTeamId == assignmentId, cancellationToken);
        }

        public IQueryable<ReliefStationTeam> GetQueryableWithTeamDetails()
        {
            return _dbSet
                .AsNoTracking()
                .Include(rst => rst.Team)
                    .ThenInclude(t => t.Leader)
                .Include(rst => rst.Team)
                    .ThenInclude(t => t.Moderator)
                .AsQueryable();
        }
    }
}
