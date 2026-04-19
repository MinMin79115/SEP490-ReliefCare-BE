using Microsoft.EntityFrameworkCore;
using ReliefManagementSystem.Application.Common.Interface;
using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Infrastructure.Data;

namespace ReliefManagementSystem.Infrastructure.Repositories
{
    public class CampaignInventoryStockRepository : GenericRepository<CampaignInventoryStock>, ICampaignInventoryStockRepository
    {
        public CampaignInventoryStockRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<List<CampaignInventoryStock>> GetByCampaignInventoryIdAsync(Guid campaignInventoryId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(x => x.CampaignInventoryId == campaignInventoryId)
                .Include(x => x.SupplyItem)
                .OrderBy(x => x.SupplyItem.Name)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<CampaignInventoryStock>> GetByCampaignInventoryIdForUpdateAsync(Guid campaignInventoryId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(x => x.CampaignInventoryId == campaignInventoryId)
                .Include(x => x.SupplyItem)
                .ToListAsync(cancellationToken);
        }

        public async Task<CampaignInventoryStock?> GetByCampaignInventoryAndSupplyItemAsync(Guid campaignInventoryId, Guid supplyItemId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .FirstOrDefaultAsync(x => x.CampaignInventoryId == campaignInventoryId && x.SupplyItemId == supplyItemId, cancellationToken);
        }
    }
}
