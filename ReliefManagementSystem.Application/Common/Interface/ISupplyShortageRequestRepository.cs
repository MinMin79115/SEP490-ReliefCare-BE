using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Application.Common.Interface
{
    public interface ISupplyShortageRequestRepository : IGenericRepository<SupplyShortageRequest>
    {
        Task<List<SupplyShortageRequest>> GetByCampaignAsync(Guid campaignId, CancellationToken cancellationToken = default);
        Task<List<SupplyShortageRequest>> GetByCampaignTeamAsync(Guid campaignTeamId, CancellationToken cancellationToken = default);
        Task<List<SupplyShortageRequest>> GetByStatusAsync(SupplyShortageRequestStatus status, CancellationToken cancellationToken = default);
        Task<SupplyShortageRequest?> GetByIdWithItemsAsync(Guid shortageRequestId, CancellationToken cancellationToken = default);
    }
}
