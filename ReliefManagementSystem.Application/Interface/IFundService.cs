using ReliefManagementSystem.Application.Features.Fund.Dtos.Responses;

namespace ReliefManagementSystem.Application.Interface
{
    public interface IFundService
    {
        Task<FundSummaryResponse> GetSummaryAsync(CancellationToken cancellationToken = default);
        Task<IReadOnlyList<FundContributionResponse>> GetContributionsAsync(CancellationToken cancellationToken = default);
        Task<IReadOnlyList<FundTransactionResponse>> GetTransactionsAsync(CancellationToken cancellationToken = default);
        Task EnsureContributionForCompletedDonationAsync(Domain.Entities.Donation donation, CancellationToken cancellationToken = default);
        Task ReverseContributionForDonationAsync(Domain.Entities.Donation donation, CancellationToken cancellationToken = default);
    }
}
