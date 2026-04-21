using Microsoft.EntityFrameworkCore;
using ReliefManagementSystem.Application.Common.Interface;
using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Infrastructure.Data;

namespace ReliefManagementSystem.Infrastructure.Repositories
{
    public class CampaignInventoryRepository : GenericRepository<CampaignInventory>, ICampaignInventoryRepository
    {
        public CampaignInventoryRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<CampaignInventory?> GetByCampaignIdWithDetailsAsync(Guid campaignId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(x => x.Campaign)
                .Include(x => x.Stocks)
                    .ThenInclude(x => x.SupplyItem)
                .Include(x => x.Transactions)
                .FirstOrDefaultAsync(x => x.CampaignId == campaignId, cancellationToken);
        }

        public async Task<CampaignInventory?> GetByIdWithDetailsAsync(Guid campaignInventoryId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(x => x.Campaign)
                .Include(x => x.Stocks)
                    .ThenInclude(x => x.SupplyItem)
                .Include(x => x.Transactions)
                .FirstOrDefaultAsync(x => x.CampaignInventoryId == campaignInventoryId, cancellationToken);
        }
    }
}
