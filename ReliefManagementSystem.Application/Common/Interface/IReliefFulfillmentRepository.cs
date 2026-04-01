using ReliefManagementSystem.Domain.Entities;

namespace ReliefManagementSystem.Application.Common.Interface
{
    public interface IReliefFulfillmentRepository : IGenericRepository<ReliefFulfillment>
    {
        Task<ReliefFulfillment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<List<ReliefFulfillment>> GetByRequestAsync(Guid reliefRequestId, CancellationToken cancellationToken = default);
        Task<List<ReliefFulfillment>> GetBySessionAsync(Guid distributionSessionId, CancellationToken cancellationToken = default);
    }
}
