using ReliefManagementSystem.Domain.Entities;

namespace ReliefManagementSystem.Application.Common.Interface
{
    public interface ICampaignInventoryStockRepository : IGenericRepository<CampaignInventoryStock>
    {
        Task<List<CampaignInventoryStock>> GetByCampaignInventoryIdAsync(Guid campaignInventoryId, CancellationToken cancellationToken = default);
        Task<List<CampaignInventoryStock>> GetByCampaignInventoryIdForUpdateAsync(Guid campaignInventoryId, CancellationToken cancellationToken = default);
        Task<CampaignInventoryStock?> GetByCampaignInventoryAndSupplyItemAsync(Guid campaignInventoryId, Guid supplyItemId, CancellationToken cancellationToken = default);
    }
}
