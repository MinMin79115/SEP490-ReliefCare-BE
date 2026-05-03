using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Application.Common.Interface
{
    public interface ICampaignTaskRepository : IGenericRepository<CampaignTask>
    {
        Task<CampaignTask?> GetByIdWithDetailsAsync(Guid campaignTaskId, CancellationToken cancellationToken = default);
        Task<(List<CampaignTask> Items, int TotalCount)> GetPagedByCampaignAsync(
            Guid campaignId,
            int pageIndex,
            int pageSize,
            CampaignTaskStatus? status,
            Guid? campaignTeamId,
            CancellationToken cancellationToken = default);
        IQueryable<CampaignTask> GetQueryable();
    }
}
