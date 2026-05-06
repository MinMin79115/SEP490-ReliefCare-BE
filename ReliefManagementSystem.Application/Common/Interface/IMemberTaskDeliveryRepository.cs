using ReliefManagementSystem.Domain.Entities;

namespace ReliefManagementSystem.Application.Common.Interface
{
    public interface IMemberTaskDeliveryRepository : IGenericRepository<MemberTaskDelivery>
    {
        IQueryable<MemberTaskDelivery> GetQueryable();
        Task<List<MemberTaskDelivery>> GetByMemberTaskIdAsync(Guid memberTaskId, CancellationToken cancellationToken = default);
    }
}
