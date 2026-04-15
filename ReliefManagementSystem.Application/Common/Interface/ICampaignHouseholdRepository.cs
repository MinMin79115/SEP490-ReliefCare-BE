using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Application.Common.Interface
{
    public interface ICampaignHouseholdRepository : IGenericRepository<CampaignHousehold>
    {
        Task<List<CampaignHousehold>> GetByCampaignAsync(Guid campaignId, CancellationToken cancellationToken = default);
        Task<List<CampaignHousehold>> GetByDistributionPointAsync(Guid distributionPointId, CancellationToken cancellationToken = default);
        Task<List<CampaignHousehold>> GetByCampaignTeamAsync(Guid campaignTeamId, CancellationToken cancellationToken = default);
        Task<List<CampaignHousehold>> GetByCampaignAndStatusAsync(
            Guid campaignId,
            HouseholdFulfillmentStatus status,
            CancellationToken cancellationToken = default);
        Task<CampaignHousehold?> GetByIdWithDeliveriesAsync(Guid campaignHouseholdId, CancellationToken cancellationToken = default);
    }
}
