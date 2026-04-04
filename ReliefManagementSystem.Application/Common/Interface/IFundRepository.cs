using ReliefManagementSystem.Domain.Entities;

namespace ReliefManagementSystem.Application.Common.Interface
{
    public interface IFundRepository : IGenericRepository<Fund>
    {
        Task<Fund?> GetDefaultAsync(CancellationToken cancellationToken = default);
        Task<Fund> GetOrCreateDefaultAsync(CancellationToken cancellationToken = default);
        Task<FundContribution?> GetContributionByDonationIdAsync(Guid donationId, CancellationToken cancellationToken = default);
        Task<List<FundContribution>> GetContributionsAsync(Guid fundId, CancellationToken cancellationToken = default);
        Task<List<FundTransaction>> GetTransactionsAsync(Guid fundId, CancellationToken cancellationToken = default);
        Task AddContributionAsync(FundContribution contribution, CancellationToken cancellationToken = default);
        Task AddTransactionAsync(FundTransaction transaction, CancellationToken cancellationToken = default);
    }
}
