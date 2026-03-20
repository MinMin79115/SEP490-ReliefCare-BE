using Microsoft.EntityFrameworkCore;
using ReliefManagementSystem.Application.Common.Interface;
using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Domain.Enum;
using ReliefManagementSystem.Infrastructure.Data;

namespace ReliefManagementSystem.Infrastructure.Repositories
{
    public class DonationRepository : GenericRepository<Donation>, IDonationRepository
    {
        public DonationRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Donation?> GetByPayOsOrderCodeAsync(long orderCode, CancellationToken cancellationToken = default)
        {
            return await _context.Donations
                .FirstOrDefaultAsync(d => d.PayOsOrderCode == orderCode, cancellationToken);
        }

        public async Task<Donation?> GetDetailByIdAsync(Guid donationId, CancellationToken cancellationToken = default)
        {
            return await _context.Donations
                .Include(d => d.Campaign)
                .FirstOrDefaultAsync(d => d.DonationId == donationId, cancellationToken);
        }

        public async Task<List<Donation>> GetPagedAsync(
            int pageIndex,
            int pageSize,
            DonationStatus? status,
            Guid? campaignId,
            string? keyword,
            DateTime? fromDate,
            DateTime? toDate,
            CancellationToken cancellationToken = default)
        {
            var query = BuildQuery(status, campaignId, keyword, fromDate, toDate);

            pageIndex = pageIndex <= 0 ? 1 : pageIndex;
            pageSize = pageSize <= 0 ? 10 : pageSize;

            return await query
                .OrderByDescending(d => d.DonatedAt)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);
        }

        public async Task<int> CountAsync(
            DonationStatus? status,
            Guid? campaignId,
            string? keyword,
            DateTime? fromDate,
            DateTime? toDate,
            CancellationToken cancellationToken = default)
        {
            return await BuildQuery(status, campaignId, keyword, fromDate, toDate)
                .CountAsync(cancellationToken);
        }

        public async Task<List<Donation>> GetAllFilteredAsync(
            DonationStatus? status,
            Guid? campaignId,
            string? keyword,
            DateTime? fromDate,
            DateTime? toDate,
            CancellationToken cancellationToken = default)
        {
            return await BuildQuery(status, campaignId, keyword, fromDate, toDate)
                .OrderByDescending(d => d.DonatedAt)
                .ToListAsync(cancellationToken);
        }

        private IQueryable<Donation> BuildQuery(
            DonationStatus? status,
            Guid? campaignId,
            string? keyword,
            DateTime? fromDate,
            DateTime? toDate)
        {
            var query = _context.Donations
                .Include(d => d.Campaign)
                .AsQueryable();

            if (status.HasValue)
            {
                query = query.Where(d => d.Status == status.Value);
            }

            if (campaignId.HasValue)
            {
                query = query.Where(d => d.CampaignId == campaignId.Value);
            }

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(d =>
                    d.DonorName.Contains(keyword) ||
                    (d.PayOsOrderCode.HasValue && d.PayOsOrderCode.Value.ToString().Contains(keyword)));
            }

            if (fromDate.HasValue)
            {
                query = query.Where(d => d.DonatedAt >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(d => d.DonatedAt <= toDate.Value);
            }

            return query;
        }
    }
}
