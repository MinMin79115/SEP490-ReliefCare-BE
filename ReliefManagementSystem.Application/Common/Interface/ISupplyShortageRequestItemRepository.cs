using ReliefManagementSystem.Domain.Entities;

namespace ReliefManagementSystem.Application.Common.Interface
{
    public interface ISupplyShortageRequestItemRepository : IGenericRepository<SupplyShortageRequestItem>
    {
        Task<List<SupplyShortageRequestItem>> GetByShortageRequestAsync(Guid shortageRequestId, CancellationToken cancellationToken = default);
    }
}
