using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Application.Common.Interface
{
    /// <summary>
    /// Repository interface for Inventory operations.
    /// </summary>
    public interface IInventoryRepository : IGenericRepository<Inventory>
    {
        /// <summary>
        /// Gets all active inventories, optionally filtered by relief station.
        /// </summary>
        Task<IReadOnlyList<Inventory>> GetAllActiveAsync(Guid? reliefStationId = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets an inventory with its stock items and supply item details included.
        /// </summary>
        Task<Inventory?> GetByIdWithStocksAsync(Guid inventoryId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets all inventories belonging to a specific relief station (active only).
        /// </summary>
        Task<IReadOnlyList<Inventory>> GetByReliefStationAsync(Guid reliefStationId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Checks whether a relief station already has an inventory of the given level.
        /// </summary>
        Task<bool> IsLevelExistsForStationAsync(Guid reliefStationId, InventoryLevel level, Guid? excludeId = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns a queryable for composing server-side filter + pagination.
        /// </summary>
        IQueryable<Inventory> GetQueryable();
    }
}
