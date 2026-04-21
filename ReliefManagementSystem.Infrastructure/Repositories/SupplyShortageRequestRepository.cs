using Microsoft.EntityFrameworkCore;
using ReliefManagementSystem.Application.Common.Interface;
using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Domain.Enum;
using ReliefManagementSystem.Infrastructure.Data;

namespace ReliefManagementSystem.Infrastructure.Repositories
{
    public class SupplyShortageRequestRepository : GenericRepository<SupplyShortageRequest>, ISupplyShortageRequestRepository
    {
        public SupplyShortageRequestRepository(ApplicationDbContext context) : base(context)
        {
        }

        public IQueryable<SupplyShortageRequest> GetQueryable()
            => _context.SupplyShortageRequests
                .Include(x => x.DistributionPoint)
                .Include(x => x.CampaignTeam)
                    .ThenInclude(ct => ct.Team)
                .Include(x => x.RequestedByUser)
                .Include(x => x.ReviewedByUser)
                .Include(x => x.Items)
                    .ThenInclude(i => i.SupplyItem)
                .AsQueryable();

        public async Task<List<SupplyShortageRequest>> GetByCampaignAsync(Guid campaignId, CancellationToken cancellationToken = default)
        {
            return await _context.SupplyShortageRequests
                .Include(x => x.CampaignTeam)
                    .ThenInclude(ct => ct.Team)
                .Include(x => x.RequestedByUser)
                .Include(x => x.ReviewedByUser)
                .Include(x => x.Items)
                .Where(x => x.CampaignId == campaignId)
                .OrderByDescending(x => x.RequestedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<SupplyShortageRequest>> GetByCampaignTeamAsync(Guid campaignTeamId, CancellationToken cancellationToken = default)
        {
            return await _context.SupplyShortageRequests
                .Include(x => x.RequestedByUser)
                .Include(x => x.ReviewedByUser)
                .Include(x => x.Items)
                .Where(x => x.CampaignTeamId == campaignTeamId)
                .OrderByDescending(x => x.RequestedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<SupplyShortageRequest>> GetByStatusAsync(SupplyShortageRequestStatus status, CancellationToken cancellationToken = default)
        {
            return await _context.SupplyShortageRequests
                .Include(x => x.CampaignTeam)
                    .ThenInclude(ct => ct.Team)
                .Include(x => x.RequestedByUser)
                .Include(x => x.ReviewedByUser)
                .Include(x => x.Items)
                .Where(x => x.Status == status)
                .OrderByDescending(x => x.RequestedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<SupplyShortageRequest?> GetByIdWithItemsAsync(Guid shortageRequestId, CancellationToken cancellationToken = default)
        {
            return await _context.SupplyShortageRequests
                .Include(x => x.Campaign)
                .Include(x => x.DistributionPoint)
                .Include(x => x.CampaignTeam)
                    .ThenInclude(ct => ct.Team)
                .Include(x => x.RequestedByUser)
                .Include(x => x.ReviewedByUser)
                .Include(x => x.Items)
                    .ThenInclude(i => i.SupplyItem)
                .FirstOrDefaultAsync(x => x.SupplyShortageRequestId == shortageRequestId, cancellationToken);
        }
    }
}
