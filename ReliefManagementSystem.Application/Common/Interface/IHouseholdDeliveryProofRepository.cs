using ReliefManagementSystem.Domain.Entities;

namespace ReliefManagementSystem.Application.Common.Interface
{
    public interface IHouseholdDeliveryProofRepository : IGenericRepository<HouseholdDeliveryProof>
    {
        Task<List<HouseholdDeliveryProof>> GetByDeliveryAsync(Guid householdDeliveryId, CancellationToken cancellationToken = default);
    }
}
