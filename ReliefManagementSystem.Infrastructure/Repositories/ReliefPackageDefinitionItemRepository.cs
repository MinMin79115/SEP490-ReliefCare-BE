using Microsoft.EntityFrameworkCore;
using ReliefManagementSystem.Application.Common.Interface;
using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Infrastructure.Data;

namespace ReliefManagementSystem.Infrastructure.Repositories
{
    public class ReliefPackageDefinitionItemRepository : GenericRepository<ReliefPackageDefinitionItem>, IReliefPackageDefinitionItemRepository
    {
        public ReliefPackageDefinitionItemRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<List<ReliefPackageDefinitionItem>> GetByPackageDefinitionAsync(Guid packageDefinitionId, CancellationToken cancellationToken = default)
        {
            return await _context.ReliefPackageDefinitionItems
                .Include(x => x.SupplyItem)
                .Where(x => x.ReliefPackageDefinitionId == packageDefinitionId)
                .OrderBy(x => x.SupplyItem.Name)
                .ToListAsync(cancellationToken);
        }
    }
}
