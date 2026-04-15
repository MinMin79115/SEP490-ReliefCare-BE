using ReliefManagementSystem.Domain.Entities;

namespace ReliefManagementSystem.Application.Common.Interface
{
    public interface IReliefPackageDefinitionItemRepository : IGenericRepository<ReliefPackageDefinitionItem>
    {
        Task<List<ReliefPackageDefinitionItem>> GetByPackageDefinitionAsync(Guid packageDefinitionId, CancellationToken cancellationToken = default);
    }
}
