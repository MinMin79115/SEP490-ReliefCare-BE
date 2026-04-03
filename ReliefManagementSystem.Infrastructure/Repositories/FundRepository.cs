using Microsoft.EntityFrameworkCore;
using ReliefManagementSystem.Application.Common.Interface;
using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Infrastructure.Data;

namespace ReliefManagementSystem.Infrastructure.Repositories
{
    public class FundRepository : GenericRepository<Fund>, IFundRepository
    {
        public FundRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Fund?> GetDefaultAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Funds.FirstOrDefaultAsync(f => f.IsDefault, cancellationToken);
        }

        public async Task<Fund> GetOrCreateDefaultAsync(CancellationToken cancellationToken = default)
        {
            var fund = await GetDefaultAsync(cancellationToken);
            if (fund != null)
            {
                return fund;
            }

            fund = new Fund
            {
                FundId = Guid.NewGuid(),
                Name = "Central Relief Fund",
                TotalBalance = 0,
                IsDefault = true,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Funds.AddAsync(fund, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return fund;
        }

        public async Task<FundContribution?> GetContributionByDonationIdAsync(Guid donationId, CancellationToken cancellationToken = default)
        {
            return await _context.FundContributions
                .Include(fc => fc.Campaign)
                .FirstOrDefaultAsync(fc => fc.DonationId == donationId, cancellationToken);
        }

        public async Task<List<FundContribution>> GetContributionsAsync(Guid fundId, CancellationToken cancellationToken = default)
        {
            return await _context.FundContributions
                .Include(fc => fc.Campaign)
                .Include(fc => fc.Donation)
                .Where(fc => fc.FundId == fundId)
                .OrderByDescending(fc => fc.ContributedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<FundTransaction>> GetTransactionsAsync(Guid fundId, CancellationToken cancellationToken = default)
        {
            return await _context.FundTransactions
                .Include(ft => ft.FundContribution)
                    .ThenInclude(fc => fc!.Campaign)
                .Where(ft => ft.FundId == fundId)
                .OrderByDescending(ft => ft.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task AddContributionAsync(FundContribution contribution, CancellationToken cancellationToken = default)
        {
            await _context.FundContributions.AddAsync(contribution, cancellationToken);
        }

        public async Task AddTransactionAsync(FundTransaction transaction, CancellationToken cancellationToken = default)
        {
            await _context.FundTransactions.AddAsync(transaction, cancellationToken);
        }
    }
}
