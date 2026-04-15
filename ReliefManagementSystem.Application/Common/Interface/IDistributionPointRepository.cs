using ReliefManagementSystem.Domain.Entities;

namespace ReliefManagementSystem.Application.Common.Interface
{
    public interface IDistributionPointRepository : IGenericRepository<DistributionPoint>
    {
        Task<List<DistributionPoint>> GetByCampaignAsync(Guid campaignId, CancellationToken cancellationToken = default);
        Task<List<DistributionPoint>> GetByCampaignTeamAsync(Guid campaignTeamId, CancellationToken cancellationToken = default);
        Task<List<DistributionPoint>> GetByStationAsync(Guid reliefStationId, CancellationToken cancellationToken = default);
        Task<List<DistributionPoint>> GetActiveByCampaignAsync(Guid campaignId, CancellationToken cancellationToken = default);
        Task<DistributionPoint?> GetByIdWithDeliveriesAsync(Guid distributionPointId, CancellationToken cancellationToken = default);
    }
}
