using Microsoft.EntityFrameworkCore;
using ReliefManagementSystem.Application.Common.Interface;
using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Infrastructure.Data;

namespace ReliefManagementSystem.Infrastructure.Repositories
{
    public class RescueBatchItemRepository : GenericRepository<RescueBatchItem>, IRescueBatchItemRepository
    {
        public RescueBatchItemRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<int> GetMaxSequenceOrderAsync(Guid batchId, CancellationToken ct = default)
        {
            return await _context.Set<RescueBatchItem>()
                .Where(i => i.RescueBatchId == batchId)
                .Select(i => (int?)i.SequenceOrder)
                .MaxAsync(ct) ?? -1;
        }
    }
}
