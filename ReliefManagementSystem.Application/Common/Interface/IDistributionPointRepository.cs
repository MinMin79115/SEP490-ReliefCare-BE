 using ReliefManagementSystem.Domain.Entities;
 using System.Linq;

namespace ReliefManagementSystem.Application.Common.Interface
{
    public interface IDistributionPointRepository : IGenericRepository<DistributionPoint>
    {
        IQueryable<DistributionPoint> GetQueryable();
        Task<List<DistributionPoint>> GetByCampaignAsync(Guid campaignId, CancellationToken cancellationToken = default);
        Task<List<DistributionPoint>> GetByCampaignTeamAsync(Guid campaignTeamId, CancellationToken cancellationToken = default);
        Task<List<DistributionPoint>> GetByStationAsync(Guid reliefStationId, CancellationToken cancellationToken = default);
        Task<List<DistributionPoint>> GetActiveByCampaignAsync(Guid campaignId, CancellationToken cancellationToken = default);
        Task<DistributionPoint?> GetByIdWithDeliveriesAsync(Guid distributionPointId, CancellationToken cancellationToken = default);
    }
}
