using ReliefManagementSystem.Application.Common.Models;
using ReliefManagementSystem.Application.Features.InventoryTransaction.DTOs.Request;
using ReliefManagementSystem.Application.Features.InventoryTransaction.DTOs.Response;
using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Application.Interface
{
    /// <summary>
    /// Service contract for inventory transactions.
    /// Transactions are immutable once created (audit trail).
    /// </summary>
    public interface IInventoryTransactionService
    {
        /// <summary>Creates a new transaction and atomically updates stock quantities.</summary>
        Task<TransactionResponse> CreateTransactionAsync(
            CreateTransactionRequest request,
            CancellationToken cancellationToken = default);

        /// <summary>Gets a transaction by ID with full line-item details.</summary>
        Task<TransactionResponse> GetTransactionByIdAsync(
            Guid transactionId,
            CancellationToken cancellationToken = default);

        /// <summary>Gets paginated transactions for a given inventory, newest first.</summary>
        Task<Pagination<TransactionSummaryResponse>> GetTransactionsByInventoryAsync(
            Guid inventoryId,
            int pageIndex = 1,
            int pageSize = 20,
            CancellationToken cancellationToken = default);

        /// <summary>Gets paginated transactions filtered by type, optionally scoped to an inventory.</summary>
        Task<Pagination<TransactionSummaryResponse>> GetTransactionsByTypeAsync(
            TransactionType type,
            Guid? inventoryId = null,
            int pageIndex = 1,
            int pageSize = 20,
            CancellationToken cancellationToken = default);
    }
}
