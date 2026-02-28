using ReliefManagementSystem.Application.Features.SupplyAllocation.DTOs.Request;
using ReliefManagementSystem.Application.Features.SupplyAllocation.DTOs.Response;
using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Application.Interface
{
    /// <summary>
    /// Service contract for SupplyAllocation workflow.
    /// Workflow: Pending → Approved → Delivered | Cancelled
    /// Stock is deducted on Approve, returned on Cancel-after-Approve.
    /// </summary>
    public interface ISupplyAllocationService
    {
        /// <summary>Creates a new allocation in Pending status. No stock change yet.</summary>
        Task<SupplyAllocationResponse> CreateAsync(CreateSupplyAllocationRequest request, CancellationToken cancellationToken = default);

        /// <summary>Gets allocation by ID with full line-item details.</summary>
        Task<SupplyAllocationResponse> GetByIdAsync(Guid allocationId, CancellationToken cancellationToken = default);

        /// <summary>Gets all allocations for a campaign.</summary>
        Task<IReadOnlyList<SupplyAllocationSummaryResponse>> GetByCampaignAsync(Guid campaignId, CancellationToken cancellationToken = default);

        /// <summary>Gets all allocations sourced from a specific inventory.</summary>
        Task<IReadOnlyList<SupplyAllocationSummaryResponse>> GetByInventoryAsync(Guid inventoryId, CancellationToken cancellationToken = default);

        /// <summary>Gets allocations filtered by status.</summary>
        Task<IReadOnlyList<SupplyAllocationSummaryResponse>> GetByStatusAsync(SupplyAllocationStatus status, CancellationToken cancellationToken = default);

        /// <summary>
        /// Transitions allocation status. Business rules:
        /// - Pending → Approved: validates and deducts stock from source inventory.
        /// - Approved → Delivered: marks as delivered.
        /// - Pending/Approved → Cancelled: if was Approved, returns stock to inventory.
        /// - All other transitions are rejected.
        /// </summary>
        Task<SupplyAllocationResponse> UpdateStatusAsync(Guid allocationId, UpdateAllocationStatusRequest request, CancellationToken cancellationToken = default);
    }
}
