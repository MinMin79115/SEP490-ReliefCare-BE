using ReliefManagementSystem.Domain.Entities;

namespace ReliefManagementSystem.Application.Common.Interface
{
    public interface ICampaignBudgetTransferRepository : IGenericRepository<CampaignBudgetTransfer>
    {
        Task<List<CampaignBudgetTransfer>> GetByCampaignAsync(Guid campaignId, CancellationToken cancellationToken = default);
    }
}
