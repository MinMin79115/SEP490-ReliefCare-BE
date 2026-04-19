using Microsoft.EntityFrameworkCore;
using ReliefManagementSystem.Application.Common.Interface;
using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Domain.Enum;
using ReliefManagementSystem.Infrastructure.Data;

namespace ReliefManagementSystem.Infrastructure.Repositories
{
    public class CampaignInventoryTransactionRepository : GenericRepository<CampaignInventoryTransaction>, ICampaignInventoryTransactionRepository
    {
        public CampaignInventoryTransactionRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<CampaignInventoryTransaction?> GetByIdWithItemsAsync(Guid campaignInventoryTransactionId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(x => x.CampaignInventory)
                    .ThenInclude(x => x.Campaign)
                .Include(x => x.CreatedByUser)
                .Include(x => x.Items)
                    .ThenInclude(x => x.SupplyItem)
                .FirstOrDefaultAsync(x => x.CampaignInventoryTransactionId == campaignInventoryTransactionId, cancellationToken);
        }

        public async Task<int> CountTodayByTypeAsync(TransactionType type, CancellationToken cancellationToken = default)
        {
            var utcDate = DateTime.UtcNow.Date;
            var nextDate = utcDate.AddDays(1);

            return await _dbSet.CountAsync(x => x.Type == type && x.CreatedAt >= utcDate && x.CreatedAt < nextDate, cancellationToken);
        }

        public IQueryable<CampaignInventoryTransaction> GetQueryable()
            => _dbSet
                .AsNoTracking()
                .Include(x => x.CampaignInventory)
                    .ThenInclude(x => x.Campaign)
                .Include(x => x.CreatedByUser)
                .Include(x => x.Items);
    }
}
