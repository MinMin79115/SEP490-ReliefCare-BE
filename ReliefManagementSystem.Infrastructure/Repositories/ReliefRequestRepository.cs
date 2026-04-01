using Microsoft.EntityFrameworkCore;
using ReliefManagementSystem.Application.Common.Interface;
using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Domain.Enum;
using ReliefManagementSystem.Infrastructure.Data;

namespace ReliefManagementSystem.Infrastructure.Repositories
{
    public class ReliefRequestRepository : GenericRepository<ReliefRequest>, IReliefRequestRepository
    {
        public ReliefRequestRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<ReliefRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.ReliefRequests
                .Where(r => r.RequestId == id)
                .Include(r => r.Campaign)
                .Include(r => r.AssignedReliefStation)
                .Include(r => r.Attachments)
                .Include(r => r.Verifications)
                .Include(r => r.ReliefNeedItems)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<(List<ReliefRequest> Items, int TotalCount)> SearchAsync(
            string? search,
            int pageNumber,
            int pageSize,
            ReliefRequestStatus? status,
            Guid? assignedStationId,
            Guid? campaignId,
            CancellationToken cancellationToken = default)
        {
            pageNumber = pageNumber <= 0 ? 1 : pageNumber;
            pageSize = pageSize <= 0 ? 10 : pageSize;

            var query = _context.ReliefRequests
                .Include(r => r.Campaign)
                .Include(r => r.AssignedReliefStation)
                .Include(r => r.Attachments)
                .Include(r => r.Verifications)
                .Include(r => r.ReliefNeedItems)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim();
                query = query.Where(r =>
                    r.ReporterFullName.Contains(keyword) ||
                    r.ReporterPhone.Contains(keyword) ||
                    r.Address.Contains(keyword) ||
                    r.Description.Contains(keyword));
            }

            if (status.HasValue)
            {
                query = query.Where(r => r.Status == status.Value);
            }

            if (assignedStationId.HasValue)
            {
                query = query.Where(r => r.AssignedReliefStationId == assignedStationId.Value);
            }

            if (campaignId.HasValue)
            {
                query = query.Where(r => r.CampaignId == campaignId.Value);
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderByDescending(r => r.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }

        public async Task<List<ReliefRequest>> GetPendingRequestsAsync(CancellationToken cancellationToken = default)
        {
            return await _context.ReliefRequests
                .Where(r => r.Status == ReliefRequestStatus.Pending)
                .Include(r => r.AssignedReliefStation)
                .Include(r => r.ReliefNeedItems)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync(cancellationToken);
        }
    }
}
