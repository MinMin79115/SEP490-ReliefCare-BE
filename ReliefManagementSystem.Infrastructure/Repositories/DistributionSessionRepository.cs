using Microsoft.EntityFrameworkCore;
using ReliefManagementSystem.Application.Common.Interface;
using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Domain.Enum;
using ReliefManagementSystem.Infrastructure.Data;

namespace ReliefManagementSystem.Infrastructure.Repositories
{
    public class DistributionSessionRepository : GenericRepository<DistributionSession>, IDistributionSessionRepository
    {
        public DistributionSessionRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<DistributionSession?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.DistributionSessions
                .Where(s => s.DistributionSessionId == id)
                .Include(s => s.Campaign)
                .Include(s => s.ReliefStation)
                .Include(s => s.Items)
                    .ThenInclude(i => i.SupplyItem)
                .Include(s => s.Items)
                    .ThenInclude(i => i.SupplyAllocationItem)
                .Include(s => s.Requests)
                    .ThenInclude(r => r.ReliefRequest)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<(List<DistributionSession> Items, int TotalCount)> SearchAsync(
            string? search,
            int pageNumber,
            int pageSize,
            DistributionSessionStatus? status,
            Guid? campaignId,
            Guid? reliefStationId,
            CancellationToken cancellationToken = default)
        {
            pageNumber = pageNumber <= 0 ? 1 : pageNumber;
            pageSize = pageSize <= 0 ? 10 : pageSize;

            var query = _context.DistributionSessions
                .Include(s => s.Campaign)
                .Include(s => s.ReliefStation)
                .Include(s => s.Items)
                .Include(s => s.Requests)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim();
                query = query.Where(s =>
                    s.Name.Contains(keyword) ||
                    (s.LocationName ?? string.Empty).Contains(keyword) ||
                    (s.Address ?? string.Empty).Contains(keyword) ||
                    (s.Notes ?? string.Empty).Contains(keyword));
            }

            if (status.HasValue)
            {
                query = query.Where(s => s.Status == status.Value);
            }

            if (campaignId.HasValue)
            {
                query = query.Where(s => s.CampaignId == campaignId.Value);
            }

            if (reliefStationId.HasValue)
            {
                query = query.Where(s => s.ReliefStationId == reliefStationId.Value);
            }

            var totalCount = await query.CountAsync(cancellationToken);
            var items = await query
                .OrderByDescending(s => s.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }

        public async Task<bool> ExistsRequestAssignmentAsync(Guid distributionSessionId, Guid reliefRequestId, CancellationToken cancellationToken = default)
        {
            return await _context.Set<DistributionSessionRequest>()
                .AnyAsync(x => x.DistributionSessionId == distributionSessionId && x.ReliefRequestId == reliefRequestId, cancellationToken);
        }

        public async Task<List<DistributionSession>> GetByCampaignAsync(Guid campaignId, CancellationToken cancellationToken = default)
        {
            return await _context.DistributionSessions
                .Where(x => x.CampaignId == campaignId)
                .Include(x => x.Items)
                .Include(x => x.Requests)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync(cancellationToken);
        }
    }
}
