using ReliefManagementSystem.Application.Common.Interface;
using ReliefManagementSystem.Application.Features.InventoryTransaction.DTOs.Request;
using ReliefManagementSystem.Application.Features.InventoryTransaction.DTOs.Response;
using ReliefManagementSystem.Application.Interface;
using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Application.Services
{
    /// <summary>
    /// Handles business logic for inventory transactions.
    /// All stock quantity updates happen atomically in a single SaveChangesAsync.
    /// </summary>
    public class InventoryTransactionService : IInventoryTransactionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;

        public InventoryTransactionService(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
        {
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
        }

        /// <inheritdoc/>
        public async Task<TransactionResponse> CreateTransactionAsync(
            CreateTransactionRequest request,
            CancellationToken cancellationToken = default)
        {
            // 1. Validate inventory exists and is usable
            var inventory = await _unitOfWork.Inventories.GetByIdAsync(request.InventoryId);
            if (inventory is null || inventory.Status == EntityStatus.Deleted)
                throw new KeyNotFoundException($"Inventory '{request.InventoryId}' was not found.");

            if (inventory.Status == EntityStatus.Inactive)
                throw new InvalidOperationException("Cannot create a transaction for an inactive inventory.");

            // 2. Reject empty Items list
            if (request.Items is null || request.Items.Count == 0)
                throw new InvalidOperationException("At least one item is required.");

            // 3. Check for duplicate SupplyItemId within the same request
            var duplicates = request.Items
                .GroupBy(i => i.SupplyItemId)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (duplicates.Count != 0)
                throw new InvalidOperationException(
                    $"Duplicate supply items in request: {string.Join(", ", duplicates)}");

            // 4. Load all stocks for this inventory in ONE query (avoid N+1)
            var inventoryStocks = await _unitOfWork.InventoryStocks
                .GetByInventoryIdAsync(request.InventoryId, cancellationToken);

            // 5. Validate each item and pre-compute new quantities
            var stockUpdates = new List<(Domain.Entities.InventoryStock Stock, int NewQty)>();

            foreach (var itemReq in request.Items)
            {
                var stock = inventoryStocks.FirstOrDefault(s => s.SupplyItemId == itemReq.SupplyItemId);

                if (stock is null)
                    throw new InvalidOperationException(
                        $"Supply item '{itemReq.SupplyItemId}' is not registered in this inventory.");

                if (request.Type == TransactionType.Export && stock.CurrentQuantity < itemReq.Quantity)
                    throw new InvalidOperationException(
                        $"Insufficient stock for '{stock.SupplyItem?.Name ?? itemReq.SupplyItemId.ToString()}'. " +
                        $"Available: {stock.CurrentQuantity}, Requested: {itemReq.Quantity}.");

                int newQty = request.Type == TransactionType.Import
                    ? stock.CurrentQuantity + itemReq.Quantity
                    : stock.CurrentQuantity - itemReq.Quantity;

                stockUpdates.Add((stock, newQty));
            }

            // 6. Generate TransactionCode: "IN-YYYYMMDD-NNN" / "OUT-YYYYMMDD-NNN"
            string code = await GenerateTransactionCodeAsync(request.Type, cancellationToken);

            // 7. Build transaction entity
            var transaction = new Domain.Entities.InventoryTransaction
            {
                TransactionId = Guid.NewGuid(),
                InventoryId = request.InventoryId,
                TransactionCode = code,
                Type = request.Type,
                Reason = request.Reason,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = _currentUser.UserId
                    ?? throw new UnauthorizedAccessException("User is not authenticated."),
                Notes = request.Notes,
                Items = request.Items.Select(i => new Domain.Entities.InventoryTransactionItem
                {
                    TransactionItemId = Guid.NewGuid(),
                    SupplyItemId = i.SupplyItemId,
                    Quantity = i.Quantity,
                    Notes = i.Notes
                }).ToList()
            };

            await _unitOfWork.InventoryTransactions.AddAsync(transaction);

            // 8. Apply all stock updates
            foreach (var (stock, newQty) in stockUpdates)
            {
                stock.CurrentQuantity = newQty;
                await _unitOfWork.InventoryStocks.UpdateAsync(stock);
            }

            // 9. Single SaveChanges — atomic commit
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // 10. Reload with navigation properties for full response
            var saved = await _unitOfWork.InventoryTransactions
                .GetByIdWithItemsAsync(transaction.TransactionId, cancellationToken);

            return MapToResponse(saved!);
        }

        /// <inheritdoc/>
        public async Task<TransactionResponse> GetTransactionByIdAsync(
            Guid transactionId,
            CancellationToken cancellationToken = default)
        {
            var transaction = await _unitOfWork.InventoryTransactions
                .GetByIdWithItemsAsync(transactionId, cancellationToken);

            if (transaction is null)
                throw new KeyNotFoundException($"Transaction '{transactionId}' was not found.");

            return MapToResponse(transaction);
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<TransactionSummaryResponse>> GetTransactionsByInventoryAsync(
            Guid inventoryId,
            CancellationToken cancellationToken = default)
        {
            if (!await _unitOfWork.Inventories.ExistsAsync(inventoryId))
                throw new KeyNotFoundException($"Inventory '{inventoryId}' was not found.");

            var transactions = await _unitOfWork.InventoryTransactions
                .GetByInventoryIdAsync(inventoryId, cancellationToken);

            return transactions.Select(MapToSummary).ToList();
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<TransactionSummaryResponse>> GetTransactionsByTypeAsync(
            TransactionType type,
            Guid? inventoryId = null,
            CancellationToken cancellationToken = default)
        {
            var transactions = await _unitOfWork.InventoryTransactions
                .GetByTypeAsync(type, inventoryId, cancellationToken);

            return transactions.Select(MapToSummary).ToList();
        }

        // ═══════════════════════════════════════════════════════════
        //  PRIVATE HELPERS
        // ═══════════════════════════════════════════════════════════

        private async Task<string> GenerateTransactionCodeAsync(
            TransactionType type,
            CancellationToken cancellationToken)
        {
            string prefix = type == TransactionType.Import ? "IN" : "OUT";
            string date = DateTime.UtcNow.ToString("yyyyMMdd");
            int count = await _unitOfWork.InventoryTransactions
                .CountTodayByTypeAsync(type, cancellationToken);

            return $"{prefix}-{date}-{(count + 1):D3}";
        }

        private static TransactionResponse MapToResponse(Domain.Entities.InventoryTransaction t) => new()
        {
            TransactionId = t.TransactionId,
            InventoryId = t.InventoryId,
            ReliefStationName = t.Inventory?.ReliefStation?.Name ?? string.Empty,
            TransactionCode = t.TransactionCode,
            Type = t.Type,
            Reason = t.Reason,
            CreatedAt = t.CreatedAt,
            CreatedBy = t.CreatedBy,
            CreatedByName = t.CreatedByUser?.DisplayName ?? string.Empty,
            Notes = t.Notes,
            Items = t.Items.Select(i => new TransactionItemResponse
            {
                TransactionItemId = i.TransactionItemId,
                SupplyItemId = i.SupplyItemId,
                SupplyItemName = i.SupplyItem?.Name ?? string.Empty,
                SupplyItemUnit = i.SupplyItem?.Unit ?? string.Empty,
                Quantity = i.Quantity,
                Notes = i.Notes
            }).ToList()
        };

        private static TransactionSummaryResponse MapToSummary(Domain.Entities.InventoryTransaction t) => new()
        {
            TransactionId = t.TransactionId,
            InventoryId = t.InventoryId,
            TransactionCode = t.TransactionCode,
            Type = t.Type,
            Reason = t.Reason,
            TotalItems = t.Items.Count,
            CreatedAt = t.CreatedAt,
            CreatedByName = t.CreatedByUser?.DisplayName ?? string.Empty,
            Notes = t.Notes
        };
    }
}
