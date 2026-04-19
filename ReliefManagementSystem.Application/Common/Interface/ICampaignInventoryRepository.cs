using ReliefManagementSystem.Domain.Entities;

namespace ReliefManagementSystem.Application.Common.Interface
{
    public interface ICampaignInventoryRepository : IGenericRepository<CampaignInventory>
    {
        Task<CampaignInventory?> GetByCampaignIdWithDetailsAsync(Guid campaignId, CancellationToken cancellationToken = default);
        Task<CampaignInventory?> GetByIdWithDetailsAsync(Guid campaignInventoryId, CancellationToken cancellationToken = default);
    }
}
