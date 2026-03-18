using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Application.Common.Interface
{
    /// <summary>
    /// Repository interface for Campaign — basic lookup operations.
    /// Full Campaign CRUD will be implemented in a separate Campaign module.
    /// </summary>
    public interface ICampaignRepository : IGenericRepository<Campaign>
    {
        Task<Campaign?> GetWithGoalsAsync(Guid campaignId, CancellationToken cancellationToken = default);
        Task<Campaign?> GetWithStationsAsync(Guid campaignId, CancellationToken cancellationToken = default);
        Task<(List<Campaign> Items, int TotalCount)> GetPagedAsync(
            int pageIndex,
            int pageSize,
            string? keyword,
            CampaignStatus? status,
            CampaignType? type,
            Guid? locationId,
            CancellationToken cancellationToken = default);

        Task<bool> HasAnyActiveStationAsync(Guid campaignId, CancellationToken cancellationToken = default);
        Task<bool> IsStationAlreadyAttachedAsync(Guid campaignId, Guid reliefStationId, CancellationToken cancellationToken = default);
        Task AddStationAsync(CampaignStation campaignStation, CancellationToken cancellationToken = default);

        Task<CampaignResourceGoal?> GetGoalAsync(Guid campaignId, CampaignResourceType resourceType, CancellationToken cancellationToken = default);
        Task<List<CampaignResourceGoal>> GetGoalsAsync(Guid campaignId, CancellationToken cancellationToken = default);
        Task AddGoalAsync(CampaignResourceGoal goal, CancellationToken cancellationToken = default);
        Task UpdateGoalAsync(CampaignResourceGoal goal, CancellationToken cancellationToken = default);
    }
}
