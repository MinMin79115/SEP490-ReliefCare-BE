using Microsoft.EntityFrameworkCore;
using ReliefManagementSystem.Application.Common.Interface;
using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Domain.Enum;
using ReliefManagementSystem.Infrastructure.Data;

namespace ReliefManagementSystem.Infrastructure.Repositories
{
    public class HouseholdDeliveryRepository : GenericRepository<HouseholdDelivery>, IHouseholdDeliveryRepository
    {
        public HouseholdDeliveryRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<List<HouseholdDelivery>> GetByCampaignAsync(Guid campaignId, CancellationToken cancellationToken = default)
        {
            return await _context.HouseholdDeliveries
                .Include(x => x.CampaignHousehold)
                .Include(x => x.ReliefPackageDefinition)
                .Where(x => x.CampaignId == campaignId)
                .OrderByDescending(x => x.ScheduledAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<HouseholdDelivery>> GetByCampaignTeamAsync(Guid campaignTeamId, CancellationToken cancellationToken = default)
        {
            return await _context.HouseholdDeliveries
                .Include(x => x.CampaignHousehold)
                .Include(x => x.ReliefPackageDefinition)
                .Where(x => x.CampaignTeamId == campaignTeamId)
                .OrderByDescending(x => x.ScheduledAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<HouseholdDelivery>> GetByChecklistAsync(
            Guid campaignId,
            Guid? campaignTeamId,
            HouseholdFulfillmentStatus? status,
            CancellationToken cancellationToken = default)
        {
            var query = _context.HouseholdDeliveries
                .Include(x => x.CampaignHousehold)
                .Include(x => x.DistributionPoint)
                .Include(x => x.ReliefPackageDefinition)
                .Where(x => x.CampaignId == campaignId)
                .AsQueryable();

            if (campaignTeamId.HasValue)
            {
                query = query.Where(x => x.CampaignTeamId == campaignTeamId.Value);
            }

            if (status.HasValue)
            {
                query = query.Where(x => x.Status == status.Value);
            }

            return await query
                .OrderBy(x => x.ScheduledAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<HouseholdDelivery?> GetByIdWithProofsAsync(Guid householdDeliveryId, CancellationToken cancellationToken = default)
        {
            return await _context.HouseholdDeliveries
                .Include(x => x.CampaignHousehold)
                .Include(x => x.DistributionPoint)
                .Include(x => x.ReliefPackageDefinition)
                    .ThenInclude(p => p.Items)
                        .ThenInclude(i => i.SupplyItem)
                .Include(x => x.Proofs)
                .FirstOrDefaultAsync(x => x.HouseholdDeliveryId == householdDeliveryId, cancellationToken);
        }
    }
}
