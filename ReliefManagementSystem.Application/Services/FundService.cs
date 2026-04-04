using ReliefManagementSystem.Application.Common.Interface;
using ReliefManagementSystem.Application.Features.Fund.Dtos.Responses;
using ReliefManagementSystem.Application.Interface;
using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Application.Services
{
    public class FundService : IFundService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public FundService(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<FundSummaryResponse> GetSummaryAsync(CancellationToken cancellationToken = default)
        {
            var fund = await _unitOfWork.Funds.GetOrCreateDefaultAsync(cancellationToken);
            var contributions = await _unitOfWork.Funds.GetContributionsAsync(fund.FundId, cancellationToken);

            var grouped = contributions
                .GroupBy(c => new { c.CampaignId, CampaignName = c.Campaign.Name })
                .Select(g => new FundContributionSourceResponse
                {
                    CampaignId = g.Key.CampaignId,
                    CampaignName = g.Key.CampaignName,
                    Amount = g.Sum(x => x.Amount),
                    Percentage = fund.TotalBalance <= 0 ? 0 : Math.Round((g.Sum(x => x.Amount) / fund.TotalBalance) * 100m, 2)
                })
                .OrderByDescending(x => x.Amount)
                .ToList();

            return new FundSummaryResponse
            {
                FundId = fund.FundId,
                Name = fund.Name,
                TotalBalance = fund.TotalBalance,
                TotalContributionCount = contributions.Count,
                TotalSourceCampaigns = grouped.Count,
                Sources = grouped
            };
        }

        public async Task<IReadOnlyList<FundContributionResponse>> GetContributionsAsync(CancellationToken cancellationToken = default)
        {
            var fund = await _unitOfWork.Funds.GetOrCreateDefaultAsync(cancellationToken);
            var contributions = await _unitOfWork.Funds.GetContributionsAsync(fund.FundId, cancellationToken);

            return contributions.Select(c => new FundContributionResponse
            {
                FundContributionId = c.FundContributionId,
                DonationId = c.DonationId,
                CampaignId = c.CampaignId,
                CampaignName = c.Campaign.Name,
                Amount = c.Amount,
                ContributedAt = c.ContributedAt
            }).ToList();
        }

        public async Task<IReadOnlyList<FundTransactionResponse>> GetTransactionsAsync(CancellationToken cancellationToken = default)
        {
            var fund = await _unitOfWork.Funds.GetOrCreateDefaultAsync(cancellationToken);
            var transactions = await _unitOfWork.Funds.GetTransactionsAsync(fund.FundId, cancellationToken);

            return transactions.Select(t => new FundTransactionResponse
            {
                FundTransactionId = t.FundTransactionId,
                Type = t.Type,
                Amount = t.Amount,
                BalanceAfter = t.BalanceAfter,
                Description = t.Description,
                CampaignId = t.FundContribution?.CampaignId,
                CampaignName = t.FundContribution?.Campaign?.Name,
                CreatedAt = t.CreatedAt
            }).ToList();
        }

        public async Task EnsureContributionForCompletedDonationAsync(Donation donation, CancellationToken cancellationToken = default)
        {
            var existing = await _unitOfWork.Funds.GetContributionByDonationIdAsync(donation.DonationId, cancellationToken);
            if (existing != null)
            {
                return;
            }

            var fund = await _unitOfWork.Funds.GetOrCreateDefaultAsync(cancellationToken);
            fund.TotalBalance += donation.Amount;

            var contribution = new FundContribution
            {
                FundContributionId = Guid.NewGuid(),
                FundId = fund.FundId,
                DonationId = donation.DonationId,
                CampaignId = donation.CampaignId,
                Amount = donation.Amount,
                ContributedAt = DateTime.UtcNow
            };

            var transaction = new FundTransaction
            {
                FundTransactionId = Guid.NewGuid(),
                FundId = fund.FundId,
                Type = FundTransactionType.Credit,
                Amount = donation.Amount,
                BalanceAfter = fund.TotalBalance,
                FundContributionId = contribution.FundContributionId,
                Description = $"Donation {donation.DonationId} from campaign {donation.CampaignId}",
                CreatedBy = _currentUserService.UserId,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Funds.UpdateAsync(fund);
            await _unitOfWork.Funds.AddContributionAsync(contribution, cancellationToken);
            await _unitOfWork.Funds.AddTransactionAsync(transaction, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task ReverseContributionForDonationAsync(Donation donation, CancellationToken cancellationToken = default)
        {
            var existing = await _unitOfWork.Funds.GetContributionByDonationIdAsync(donation.DonationId, cancellationToken);
            if (existing == null)
            {
                return;
            }

            var fund = await _unitOfWork.Funds.GetOrCreateDefaultAsync(cancellationToken);
            if (fund.TotalBalance - existing.Amount < 0)
            {
                throw new InvalidOperationException("Không thể reverse fund contribution vì sẽ làm quỹ âm.");
            }

            fund.TotalBalance -= existing.Amount;

            var transaction = new FundTransaction
            {
                FundTransactionId = Guid.NewGuid(),
                FundId = fund.FundId,
                Type = FundTransactionType.Debit,
                Amount = existing.Amount,
                BalanceAfter = fund.TotalBalance,
                FundContributionId = existing.FundContributionId,
                Description = $"Reverse donation {donation.DonationId} from campaign {donation.CampaignId}",
                CreatedBy = _currentUserService.UserId,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Funds.UpdateAsync(fund);
            await _unitOfWork.Funds.AddTransactionAsync(transaction, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
