using ReliefManagementSystem.Application.Features.Inventory;

namespace ReliefManagementSystem.Application.Services
{
    public interface IInventoryService
    {
        // Dashboard
        Task<DashboardStatsDto> GetDashboardStatsAsync(CancellationToken cancellationToken);

        // Inventory Items
        Task<List<InventoryItemDto>> GetAllItemsAsync(Guid? categoryId, CancellationToken cancellationToken);
        Task<InventoryItemDto?> GetItemByIdAsync(Guid itemId, CancellationToken cancellationToken);
        Task<InventoryItemDto> CreateItemAsync(CreateInventoryItemRequest request, CancellationToken cancellationToken);
        Task<InventoryItemDto> UpdateItemAsync(Guid itemId, UpdateInventoryItemRequest request, CancellationToken cancellationToken);
        Task DeleteItemAsync(Guid itemId, CancellationToken cancellationToken);

        // Bulk Transactions
        Task<BulkTransactionResponse> BulkImportAsync(BulkImportRequest request, Guid userId, CancellationToken cancellationToken);
        Task<BulkTransactionResponse> BulkExportAsync(BulkExportRequest request, Guid userId, CancellationToken cancellationToken);

        // Batches
        Task<List<BatchDto>> GetBatchesAsync(Domain.Enum.TransactionType? type, int page, int pageSize, CancellationToken cancellationToken);
        Task<BatchDetailDto?> GetBatchDetailAsync(Guid batchId, CancellationToken cancellationToken);

        // Categories
        Task<List<CategoryDto>> GetCategoriesAsync(CancellationToken cancellationToken);
    }
}
