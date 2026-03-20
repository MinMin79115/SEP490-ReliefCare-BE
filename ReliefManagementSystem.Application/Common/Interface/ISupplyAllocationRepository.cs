using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Application.Common.Interface
{
    /// <summary>
    /// Repository interface for SupplyAllocation operations.
    /// </summary>
    public interface ISupplyAllocationRepository : IGenericRepository<SupplyAllocation>
    {
        /// <summary>
        /// Gets all allocations for a campaign, including items and source inventory.
        /// </summary>
        Task<IReadOnlyList<SupplyAllocation>> GetByCampaignIdAsync(Guid campaignId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets all allocations from a source inventory, ordered by AllocatedAt descending.
        /// </summary>
        Task<IReadOnlyList<SupplyAllocation>> GetByInventoryIdAsync(Guid inventoryId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a single allocation with full detail (items + supply item info + campaign + inventory).
        /// </summary>
        Task<SupplyAllocation?> GetByIdWithDetailsAsync(Guid allocationId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets allocations filtered by status.
        /// </summary>
        Task<IReadOnlyList<SupplyAllocation>> GetByStatusAsync(SupplyAllocationStatus status, CancellationToken cancellationToken = default);
    }
}
