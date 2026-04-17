 using ReliefManagementSystem.Domain.Entities;
 using System.Linq;

namespace ReliefManagementSystem.Application.Common.Interface
{
    public interface IReliefPackageDefinitionRepository : IGenericRepository<ReliefPackageDefinition>
    {
        IQueryable<ReliefPackageDefinition> GetQueryable();
        Task<List<ReliefPackageDefinition>> GetByCampaignAsync(Guid campaignId, CancellationToken cancellationToken = default);
        Task<ReliefPackageDefinition?> GetDefaultByCampaignAsync(Guid campaignId, CancellationToken cancellationToken = default);
        Task<ReliefPackageDefinition?> GetByIdWithItemsAsync(Guid packageDefinitionId, CancellationToken cancellationToken = default);
    }
}
