using ReliefManagementSystem.Domain.Entities;

namespace ReliefManagementSystem.Application.Common.Interface
{
    public interface IReliefPackageAssemblyRepository : IGenericRepository<ReliefPackageAssembly>
    {
        Task<ReliefPackageAssembly?> GetByIdWithDetailsAsync(Guid reliefPackageAssemblyId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<ReliefPackageAssembly>> GetByInventoryAsync(Guid inventoryId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<ReliefPackageAssembly>> GetByCampaignAsync(Guid campaignId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<ReliefPackageAssembly>> GetByStationAsync(Guid campaignId, Guid reliefStationId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<ReliefPackageAssembly>> GetByPackageDefinitionAsync(Guid campaignId, Guid reliefPackageDefinitionId, CancellationToken cancellationToken = default);
    }
}
