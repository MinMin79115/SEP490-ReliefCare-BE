using Microsoft.EntityFrameworkCore;
using ReliefManagementSystem.Application.Common.Interface;
using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Infrastructure.Data;

namespace ReliefManagementSystem.Infrastructure.Repositories
{
    public class ReliefPackageDefinitionRepository : GenericRepository<ReliefPackageDefinition>, IReliefPackageDefinitionRepository
    {
        public ReliefPackageDefinitionRepository(ApplicationDbContext context) : base(context)
        {
        }

        public IQueryable<ReliefPackageDefinition> GetQueryable()
            => _context.ReliefPackageDefinitions
                .Include(x => x.OutputSupplyItem)
                .Include(x => x.Items)
                    .ThenInclude(i => i.SupplyItem)
                .AsQueryable();

        public async Task<List<ReliefPackageDefinition>> GetByCampaignAsync(Guid campaignId, CancellationToken cancellationToken = default)
        {
            return await _context.ReliefPackageDefinitions
                .Include(x => x.OutputSupplyItem)
                .Where(x => x.CampaignId == campaignId)
                .OrderByDescending(x => x.IsDefault)
                .ThenBy(x => x.Name)
                .ToListAsync(cancellationToken);
        }

        public async Task<ReliefPackageDefinition?> GetDefaultByCampaignAsync(Guid campaignId, CancellationToken cancellationToken = default)
        {
            return await _context.ReliefPackageDefinitions
                .Include(x => x.OutputSupplyItem)
                .Include(x => x.Items)
                    .ThenInclude(i => i.SupplyItem)
                .FirstOrDefaultAsync(x => x.CampaignId == campaignId && x.IsDefault && x.IsActive, cancellationToken);
        }

        public async Task<ReliefPackageDefinition?> GetByIdWithItemsAsync(Guid packageDefinitionId, CancellationToken cancellationToken = default)
        {
            return await _context.ReliefPackageDefinitions
                .Include(x => x.OutputSupplyItem)
                .Include(x => x.Items)
                    .ThenInclude(i => i.SupplyItem)
                .FirstOrDefaultAsync(x => x.ReliefPackageDefinitionId == packageDefinitionId, cancellationToken);
        }
    }
}
