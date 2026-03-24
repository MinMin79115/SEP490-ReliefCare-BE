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
                .OrderByDescending(b => b.CreatedAt)
                .FirstOrDefaultAsync(ct);
        }

        public async Task<RescueBatch?> GetByIdWithItemsAsync(Guid batchId, CancellationToken ct = default)
        {
            return await _context.Set<RescueBatch>()
                .Where(b => b.RescueBatchId == batchId)
                .Include(b => b.Items)
                .FirstOrDefaultAsync(ct);
        }
    }
}
