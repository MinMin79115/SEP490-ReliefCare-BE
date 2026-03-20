using ReliefManagementSystem.Domain.Entities;

namespace ReliefManagementSystem.Application.Common.Interface
{
    /// <summary>
    /// Repository interface for InventoryStock operations.
    /// </summary>
    public interface IInventoryStockRepository : IGenericRepository<InventoryStock>
    {
        /// <summary>
        /// Gets all stock entries for a specific inventory, including supply item details.
        /// </summary>
        Task<IReadOnlyList<InventoryStock>> GetByInventoryIdAsync(Guid inventoryId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a single stock entry for a given inventory and supply item combination.
        /// Returns null if not found.
        /// </summary>
        Task<InventoryStock?> GetByInventoryAndSupplyItemAsync(Guid inventoryId, Guid supplyItemId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a stock entry with full navigation properties (Inventory + SupplyItem).
        /// </summary>
        Task<InventoryStock?> GetByIdWithDetailsAsync(Guid stockId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Checks whether a supply item is already registered in the given inventory.
        /// </summary>
        Task<bool> IsSupplyItemExistsInInventoryAsync(Guid inventoryId, Guid supplyItemId, Guid? excludeId = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns a queryable for composing server-side filter + pagination.
        /// Includes SupplyItem navigation by default.
        /// </summary>
        IQueryable<InventoryStock> GetQueryable();
    }
}
