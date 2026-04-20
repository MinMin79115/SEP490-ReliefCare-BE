using Microsoft.EntityFrameworkCore;
using ReliefManagementSystem.Application.Common.Interface;
using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Infrastructure.Data;

namespace ReliefManagementSystem.Infrastructure.Repositories
{
    public class RescueBatchRepository : GenericRepository<RescueBatch>, IRescueBatchRepository
    {
        public RescueBatchRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<RescueBatch?> GetActiveByTeamIdAsync(Guid teamId, CancellationToken ct = default)
        {
            return await _context.Set<RescueBatch>()
                .Where(b => b.TeamId == teamId && b.IsActive)
                .Include(b => b.Items)
                    .ThenInclude(i => i.RescueRequest)
                        .ThenInclude(r => r.RescueOperations)
                            .ThenInclude(ro => ro.Vehicle)
                                .ThenInclude(v => v.VehicleType)
                .OrderByDescending(b => b.CreatedAt)
                .FirstOrDefaultAsync(ct);
        }

        public async Task<List<RescueBatch>> GetAllActiveWithItemsAsync(CancellationToken ct = default)
        {
            return await _context.Set<RescueBatch>()
                .Where(b => b.IsActive)
                .Include(b => b.Items)
                .ToListAsync(ct);
        }

        public async Task<RescueBatch?> GetByIdWithItemsAsync(Guid batchId, CancellationToken ct = default)
        {
            return await _context.Set<RescueBatch>()
                .Where(b => b.RescueBatchId == batchId)
                .Include(b => b.Items)
                    .ThenInclude(i => i.RescueRequest)
                        .ThenInclude(r => r.RescueOperations)
                            .ThenInclude(ro => ro.Vehicle)
                                .ThenInclude(v => v.VehicleType)
                .FirstOrDefaultAsync(ct);
        }

        public async Task<(List<RescueBatch> Items, int TotalCount)> GetCompletedByTeamIdAsync(
            Guid teamId,
            int pageNumber,
            int pageSize,
            CancellationToken ct = default)
        {
            var query = _context.Set<RescueBatch>()
                .Where(b => b.TeamId == teamId && !b.IsActive);

            var totalCount = await query.CountAsync(ct);

            var items = await query
                .OrderByDescending(b => b.ClosedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Include(b => b.Items)
                    .ThenInclude(i => i.RescueRequest)
                        .ThenInclude(r => r.RescueOperations)
                            .ThenInclude(ro => ro.Vehicle)
                                .ThenInclude(v => v.VehicleType)
                .ToListAsync(ct);

            return (items, totalCount);
        }
    }
}
