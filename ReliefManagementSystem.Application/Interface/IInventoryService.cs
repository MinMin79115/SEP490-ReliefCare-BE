using ReliefManagementSystem.Application.Features.Inventory.DTOs.Request;
using ReliefManagementSystem.Application.Features.Inventory.DTOs.Response;
using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Application.Interface
{
    /// <summary>
    /// Service contract for managing inventories and their stock items.
    /// </summary>
    public interface IInventoryService
    {
        // ─── Inventory CRUD ────────────────────────────────────────────────────

        /// <summary>Creates a new inventory for a relief station.</summary>
        Task<InventoryResponse> CreateInventoryAsync(CreateInventoryRequest request, CancellationToken cancellationToken = default);

        /// <summary>Gets an inventory by ID with full stock details.</summary>
        Task<InventoryDetailResponse> GetInventoryByIdAsync(Guid inventoryId, CancellationToken cancellationToken = default);

        /// <summary>Gets all active inventories, optionally filtered by relief station.</summary>
        Task<IReadOnlyList<InventoryResponse>> GetAllInventoriesAsync(Guid? reliefStationId = null, CancellationToken cancellationToken = default);

        /// <summary>Updates level and status of an existing inventory.</summary>
        Task<InventoryResponse> UpdateInventoryAsync(Guid inventoryId, UpdateInventoryRequest request, CancellationToken cancellationToken = default);

        /// <summary>Soft-deletes an inventory (sets Status = Deleted).</summary>
        Task<bool> DeleteInventoryAsync(Guid inventoryId, CancellationToken cancellationToken = default);

        // ─── Stock Management ──────────────────────────────────────────────────

        /// <summary>Adds a supply item slot to an inventory.</summary>
        Task<InventoryStockResponse> AddStockItemAsync(Guid inventoryId, AddStockItemRequest request, CancellationToken cancellationToken = default);

        /// <summary>Gets all stock entries for a given inventory.</summary>
        Task<IReadOnlyList<InventoryStockResponse>> GetStocksByInventoryIdAsync(Guid inventoryId, CancellationToken cancellationToken = default);

        /// <summary>Updates the Min/Max stock thresholds for a stock entry.</summary>
        Task<InventoryStockResponse> UpdateStockItemAsync(Guid stockId, UpdateStockItemRequest request, CancellationToken cancellationToken = default);

        /// <summary>Removes a supply item slot from an inventory.</summary>
        Task<bool> RemoveStockItemAsync(Guid stockId, CancellationToken cancellationToken = default);
    }
}
