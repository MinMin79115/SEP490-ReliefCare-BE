using ReliefManagementSystem.Application.Common.Models;
using ReliefManagementSystem.Application.Features.Inventory.DTOs;
using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Application.Interface
{
    public interface IInventoryService
    {
        // Supply Item CRUD
        Task<SupplyItemDto> CreateSupplyItemAsync(
            CreateSupplyItemRequest request,
            CancellationToken cancellationToken = default);

        Task<SupplyItemDto> UpdateSupplyItemAsync(
            Guid id,
            UpdateSupplyItemRequest request,
            CancellationToken cancellationToken = default);

        Task<bool> DeleteSupplyItemAsync(
            Guid id,
            CancellationToken cancellationToken = default);

        Task<SupplyItemDto?> GetSupplyItemByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default);

        Task<Pagination<SupplyItemDto>> GetSupplyItemsAsync(
            SupplyCategory? category,
            InventoryStatus? status,
            string? search,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default);

        // Bulk Operations
        Task<BulkTransactionResult> BulkImportAsync(
            BulkImportRequest request,
            CancellationToken cancellationToken = default);

        Task<BulkTransactionResult> BulkExportAsync(
            BulkExportRequest request,
            CancellationToken cancellationToken = default);
    }
}
