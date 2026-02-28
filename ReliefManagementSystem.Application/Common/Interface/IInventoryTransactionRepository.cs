using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Application.Common.Interface
{
    /// <summary>
    /// Repository interface for InventoryTransaction operations.
    /// </summary>
    public interface IInventoryTransactionRepository : IGenericRepository<InventoryTransaction>
    {
        /// <summary>
        /// Gets all transactions for a specific inventory, newest first.
        /// Includes line items and supply item details.
        /// </summary>
        Task<IReadOnlyList<InventoryTransaction>> GetByInventoryIdAsync(
            Guid inventoryId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a single transaction with its items and supply item details.
        /// </summary>
        Task<InventoryTransaction?> GetByIdWithItemsAsync(
            Guid transactionId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets transactions filtered by type, optionally scoped to an inventory.
        /// </summary>
        Task<IReadOnlyList<InventoryTransaction>> GetByTypeAsync(
            TransactionType type,
            Guid? inventoryId = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Counts today's transactions of a given type — used for TransactionCode generation.
        /// </summary>
        Task<int> CountTodayByTypeAsync(
            TransactionType type,
            CancellationToken cancellationToken = default);
    }
}
