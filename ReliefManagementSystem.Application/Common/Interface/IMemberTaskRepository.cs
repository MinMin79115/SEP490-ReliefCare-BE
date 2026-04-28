using ReliefManagementSystem.Domain.Entities;

namespace ReliefManagementSystem.Application.Common.Interface
{
    public interface IMemberTaskRepository : IGenericRepository<MemberTask>
    {
        Task<List<MemberTask>> GetByCampaignTaskIdAsync(Guid campaignTaskId, CancellationToken cancellationToken = default);
        Task<MemberTask?> GetByIdWithDetailsAsync(Guid memberTaskId, CancellationToken cancellationToken = default);
        IQueryable<MemberTask> GetQueryable();
    }
}
