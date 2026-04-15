using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Application.Common.Interface
{
    public interface IHouseholdDeliveryRepository : IGenericRepository<HouseholdDelivery>
    {
        Task<List<HouseholdDelivery>> GetByCampaignAsync(Guid campaignId, CancellationToken cancellationToken = default);
        Task<List<HouseholdDelivery>> GetByCampaignTeamAsync(Guid campaignTeamId, CancellationToken cancellationToken = default);
        Task<List<HouseholdDelivery>> GetByChecklistAsync(Guid campaignId, Guid? campaignTeamId, HouseholdFulfillmentStatus? status, CancellationToken cancellationToken = default);
        Task<HouseholdDelivery?> GetByIdWithProofsAsync(Guid householdDeliveryId, CancellationToken cancellationToken = default);
    }
}
