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

        public async Task<List<CampaignBudgetTransfer>> GetByCampaignAsync(Guid campaignId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(x => x.SourceCampaignId == campaignId || x.TargetCampaignId == campaignId)
                .OrderByDescending(x => x.TransferredAt)
                .ToListAsync(cancellationToken);
        }
    }
}
