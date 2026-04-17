using Microsoft.EntityFrameworkCore;
using ReliefManagementSystem.Application.Common.Interface;
using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Infrastructure.Data;

namespace ReliefManagementSystem.Infrastructure.Repositories
{
    public class DistributionPointRepository : GenericRepository<DistributionPoint>, IDistributionPointRepository
    {
        public DistributionPointRepository(ApplicationDbContext context) : base(context)
        {
        }

        public IQueryable<DistributionPoint> GetQueryable()
            => _context.DistributionPoints.AsQueryable();

        public async Task<List<DistributionPoint>> GetByCampaignAsync(Guid campaignId, CancellationToken cancellationToken = default)
        {
            return await _context.DistributionPoints
                .Where(x => x.CampaignId == campaignId)
                .OrderBy(x => x.StartsAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<DistributionPoint>> GetByCampaignTeamAsync(Guid campaignTeamId, CancellationToken cancellationToken = default)
        {
            return await _context.DistributionPoints
                .Where(x => x.CampaignTeamId == campaignTeamId)
                .OrderBy(x => x.StartsAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<DistributionPoint>> GetByStationAsync(Guid reliefStationId, CancellationToken cancellationToken = default)
        {
            return await _context.DistributionPoints
                .Where(x => x.ReliefStationId == reliefStationId)
                .OrderByDescending(x => x.StartsAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<DistributionPoint>> GetActiveByCampaignAsync(Guid campaignId, CancellationToken cancellationToken = default)
        {
            return await _context.DistributionPoints
                .Where(x => x.CampaignId == campaignId && x.IsActive)
                .OrderBy(x => x.StartsAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<DistributionPoint?> GetByIdWithDeliveriesAsync(Guid distributionPointId, CancellationToken cancellationToken = default)
        {
            return await _context.DistributionPoints
                .Include(x => x.Households)
                .Include(x => x.Deliveries)
                    .ThenInclude(d => d.CampaignHousehold)
                .FirstOrDefaultAsync(x => x.DistributionPointId == distributionPointId, cancellationToken);
        }
    }
}
