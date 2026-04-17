using ReliefManagementSystem.Domain.Entities;

namespace ReliefManagementSystem.Application.Common.Interface
{
    public interface IReliefPackageAssemblyDetailRepository : IGenericRepository<ReliefPackageAssemblyDetail>
    {
        Task<IReadOnlyList<ReliefPackageAssemblyDetail>> GetByAssemblyIdAsync(Guid reliefPackageAssemblyId, CancellationToken cancellationToken = default);
    }
}
