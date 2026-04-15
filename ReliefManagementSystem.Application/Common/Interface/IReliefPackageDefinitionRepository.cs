using ReliefManagementSystem.Domain.Entities;

namespace ReliefManagementSystem.Application.Common.Interface
{
    public interface IReliefPackageDefinitionRepository : IGenericRepository<ReliefPackageDefinition>
    {
        Task<List<ReliefPackageDefinition>> GetByCampaignAsync(Guid campaignId, CancellationToken cancellationToken = default);
        Task<ReliefPackageDefinition?> GetDefaultByCampaignAsync(Guid campaignId, CancellationToken cancellationToken = default);
        Task<ReliefPackageDefinition?> GetByIdWithItemsAsync(Guid packageDefinitionId, CancellationToken cancellationToken = default);
    }
}
