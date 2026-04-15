using Microsoft.EntityFrameworkCore;
using ReliefManagementSystem.Application.Common.Interface;
using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Infrastructure.Data;

namespace ReliefManagementSystem.Infrastructure.Repositories
{
    public class SupplyShortageRequestItemRepository : GenericRepository<SupplyShortageRequestItem>, ISupplyShortageRequestItemRepository
    {
        public SupplyShortageRequestItemRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<List<SupplyShortageRequestItem>> GetByShortageRequestAsync(Guid shortageRequestId, CancellationToken cancellationToken = default)
        {
            return await _context.SupplyShortageRequestItems
                .Include(x => x.SupplyItem)
                .Where(x => x.SupplyShortageRequestId == shortageRequestId)
                .OrderBy(x => x.SupplyItem.Name)
                .ToListAsync(cancellationToken);
        }
    }
}
