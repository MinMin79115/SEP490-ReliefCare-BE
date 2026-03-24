using Microsoft.EntityFrameworkCore;
using ReliefManagementSystem.Application.Common.Interface;
using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Infrastructure.Data;

namespace ReliefManagementSystem.Infrastructure.Repositories
{
    public class TeamTrackingPointRepository : GenericRepository<TeamTrackingPoint>, ITeamTrackingPointRepository
    {
        public TeamTrackingPointRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<List<TeamTrackingPoint>> GetLatestByTeamAsync(
            Guid teamId,
            int limit = 100,
            CancellationToken cancellationToken = default)
        {
            return await _context.Set<TeamTrackingPoint>()
                .Where(tp => tp.TeamId == teamId)
                .OrderByDescending(tp => tp.CapturedAtUtc)
                .Take(limit)
                .ToListAsync(cancellationToken);
        }

        public async Task<TeamTrackingPoint?> GetLatestPointAsync(
            Guid teamId,
            CancellationToken cancellationToken = default)
        {
            return await _context.Set<TeamTrackingPoint>()
                .Where(tp => tp.TeamId == teamId)
                .OrderByDescending(tp => tp.CapturedAtUtc)
                .FirstOrDefaultAsync(cancellationToken);
        }
    }
}
