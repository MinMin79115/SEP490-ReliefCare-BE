using Microsoft.EntityFrameworkCore;
using ReliefManagementSystem.Application.Common.Interface;
using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Domain.Enum;
using ReliefManagementSystem.Infrastructure.Data;

namespace ReliefManagementSystem.Infrastructure.Repositories
{
    public class CampaignHouseholdRepository : GenericRepository<CampaignHousehold>, ICampaignHouseholdRepository
    {
        public CampaignHouseholdRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<List<CampaignHousehold>> GetByCampaignAsync(Guid campaignId, CancellationToken cancellationToken = default)
        {
            return await _context.CampaignHouseholds
                .Where(x => x.CampaignId == campaignId)
                .OrderBy(x => x.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<CampaignHousehold>> GetByDistributionPointAsync(Guid distributionPointId, CancellationToken cancellationToken = default)
        {
            return await _context.CampaignHouseholds
                .Where(x => x.DistributionPointId == distributionPointId)
                .OrderBy(x => x.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<CampaignHousehold>> GetByCampaignTeamAsync(Guid campaignTeamId, CancellationToken cancellationToken = default)
        {
            return await _context.CampaignHouseholds
                .Where(x => x.CampaignTeamId == campaignTeamId)
                .OrderBy(x => x.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<CampaignHousehold>> GetByCampaignAndStatusAsync(
            Guid campaignId,
            HouseholdFulfillmentStatus status,
            CancellationToken cancellationToken = default)
        {
            return await _context.CampaignHouseholds
                .Where(x => x.CampaignId == campaignId && x.FulfillmentStatus == status)
                .OrderBy(x => x.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<CampaignHousehold?> GetByIdWithDeliveriesAsync(Guid campaignHouseholdId, CancellationToken cancellationToken = default)
        {
            return await _context.CampaignHouseholds
                .Include(x => x.DistributionPoint)
                .Include(x => x.Deliveries)
                    .ThenInclude(d => d.Proofs)
                .FirstOrDefaultAsync(x => x.CampaignHouseholdId == campaignHouseholdId, cancellationToken);
        }
    }
}
