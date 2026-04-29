using Microsoft.EntityFrameworkCore;
using ReliefManagementSystem.Application.Common.Interface;
using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Infrastructure.Data;

namespace ReliefManagementSystem.Infrastructure.Repositories
{
    public class CampaignBudgetTransferRepository : GenericRepository<CampaignBudgetTransfer>, ICampaignBudgetTransferRepository
    {
        public CampaignBudgetTransferRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<List<CampaignBudgetTransfer>> GetByCampaignAsync(Guid campaignId, bool includeDeleted = false, CancellationToken cancellationToken = default)
        {
            var query = _dbSet
                .AsNoTracking()
                .Include(x => x.TransferredByUser)
                .Include(x => x.CancelledByUser)
                .Where(x => x.SourceCampaignId == campaignId || x.TargetCampaignId == campaignId);

            if (!includeDeleted)
            {
                query = query.Where(x => !x.IsDeleted);
            }

            return await query
                .OrderByDescending(x => x.TransferredAt)
                .ToListAsync(cancellationToken);
        }
    }
}
