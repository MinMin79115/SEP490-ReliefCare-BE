using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Application.Common.Interface
{
    public interface ICampaignInventoryTransactionRepository : IGenericRepository<CampaignInventoryTransaction>
    {
        Task<CampaignInventoryTransaction?> GetByIdWithItemsAsync(Guid campaignInventoryTransactionId, CancellationToken cancellationToken = default);
        Task<int> CountTodayByTypeAsync(TransactionType type, CancellationToken cancellationToken = default);
        IQueryable<CampaignInventoryTransaction> GetQueryable();
    }
}
