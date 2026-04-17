 using ReliefManagementSystem.Domain.Entities;
 using ReliefManagementSystem.Domain.Enum;
 using System.Linq;

namespace ReliefManagementSystem.Application.Common.Interface
{
    public interface ICampaignHouseholdRepository : IGenericRepository<CampaignHousehold>
    {
        IQueryable<CampaignHousehold> GetQueryable();
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
