using Microsoft.EntityFrameworkCore;
using ReliefManagementSystem.Application.Common.Interface;
using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Infrastructure.Data;

namespace ReliefManagementSystem.Infrastructure.Repositories
{
    public class ReliefPackageAssemblyDetailRepository : GenericRepository<ReliefPackageAssemblyDetail>, IReliefPackageAssemblyDetailRepository
    {
        public ReliefPackageAssemblyDetailRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IReadOnlyList<ReliefPackageAssemblyDetail>> GetByAssemblyIdAsync(Guid reliefPackageAssemblyId, CancellationToken cancellationToken = default)
        {
            return await _context.Set<ReliefPackageAssemblyDetail>()
                .AsNoTracking()
                .Include(x => x.SupplyItem)
                .Where(x => x.ReliefPackageAssemblyId == reliefPackageAssemblyId)
                .OrderBy(x => x.SupplyItem.Name)
                .ToListAsync(cancellationToken);
        }
    }
}
