using Microsoft.EntityFrameworkCore;
using ReliefManagementSystem.Application.Common.Interface;
using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Infrastructure.Data;

namespace ReliefManagementSystem.Infrastructure.Repositories
{
    public class ReliefFulfillmentRepository : GenericRepository<ReliefFulfillment>, IReliefFulfillmentRepository
    {
        public ReliefFulfillmentRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<ReliefFulfillment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Set<ReliefFulfillment>()
                .Where(x => x.ReliefFulfillmentId == id)
                .Include(x => x.ReliefRequest)
                .Include(x => x.DistributionSession)
                .Include(x => x.Items)
                    .ThenInclude(i => i.SupplyItem)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<List<ReliefFulfillment>> GetByRequestAsync(Guid reliefRequestId, CancellationToken cancellationToken = default)
        {
            return await _context.Set<ReliefFulfillment>()
                .Where(x => x.ReliefRequestId == reliefRequestId)
                .Include(x => x.ReliefRequest)
                .Include(x => x.DistributionSession)
                .Include(x => x.Items)
                    .ThenInclude(i => i.SupplyItem)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<ReliefFulfillment>> GetBySessionAsync(Guid distributionSessionId, CancellationToken cancellationToken = default)
        {
            return await _context.Set<ReliefFulfillment>()
                .Where(x => x.DistributionSessionId == distributionSessionId)
                .Include(x => x.ReliefRequest)
                .Include(x => x.DistributionSession)
                .Include(x => x.Items)
                    .ThenInclude(i => i.SupplyItem)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync(cancellationToken);
        }
    }
}
