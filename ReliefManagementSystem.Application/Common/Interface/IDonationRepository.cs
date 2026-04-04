using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Application.Common.Interface
{
    public interface IDonationRepository : IGenericRepository<Donation>
    {
        Task<Donation?> GetByPayOsOrderCodeAsync(long orderCode, CancellationToken cancellationToken = default);
        Task<Donation?> GetDetailByIdAsync(Guid donationId, CancellationToken cancellationToken = default);
        Task<List<Donation>> GetPagedAsync(
            int pageIndex,
            int pageSize,
            DonationStatus? status,
            Guid? campaignId,
            string? keyword,
            DateTime? fromDate,
            DateTime? toDate,
            CancellationToken cancellationToken = default);
        Task<int> CountAsync(
            DonationStatus? status,
            Guid? campaignId,
            string? keyword,
            DateTime? fromDate,
            DateTime? toDate,
            CancellationToken cancellationToken = default);
        Task<List<Donation>> GetAllFilteredAsync(
            DonationStatus? status,
            Guid? campaignId,
            string? keyword,
            DateTime? fromDate,
            DateTime? toDate,
            CancellationToken cancellationToken = default);
    }
}
