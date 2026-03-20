using ReliefManagementSystem.Application.Common.Models;
using ReliefManagementSystem.Application.Features.SupplyItem.DTOs.Request;
using ReliefManagementSystem.Application.Features.SupplyItem.DTOs.Response;
using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Application.Interface
{
    /// <summary>
    /// Service contract for managing supply item master data.
    /// </summary>
    public interface ISupplyItemService
    {
        /// <summary>Creates a new supply item.</summary>
        Task<SupplyItemResponse> CreateSupplyItemAsync(CreateSupplyItemRequest request, CancellationToken cancellationToken = default);

        /// <summary>Gets a supply item by its ID.</summary>
        Task<SupplyItemResponse> GetSupplyItemByIdAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>Gets paginated supply items, optionally filtered by category.</summary>
        Task<Pagination<SupplyItemResponse>> GetAllSupplyItemsAsync(
            SupplyCategory? category = null,
            int pageIndex = 1,
            int pageSize = 20,
            CancellationToken cancellationToken = default);

        /// <summary>Updates an existing supply item.</summary>
        Task<SupplyItemResponse> UpdateSupplyItemAsync(Guid id, UpdateSupplyItemRequest request, CancellationToken cancellationToken = default);

        /// <summary>Deletes a supply item by its ID (hard delete).</summary>
        Task<bool> DeleteSupplyItemAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
