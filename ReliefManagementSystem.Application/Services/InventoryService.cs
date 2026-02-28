using ReliefManagementSystem.Application.Common.Interface;
using ReliefManagementSystem.Application.Features.Inventory.DTOs.Request;
using ReliefManagementSystem.Application.Features.Inventory.DTOs.Response;
using ReliefManagementSystem.Application.Interface;
using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Application.Services
{
    /// <summary>
    /// Handles business logic for inventory and stock management.
    /// </summary>
    public class InventoryService : IInventoryService
    {
        private readonly IUnitOfWork _unitOfWork;

        public InventoryService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // ═══════════════════════════════════════════════════════════
        //  INVENTORY CRUD
        // ═══════════════════════════════════════════════════════════

        /// <inheritdoc/>
        public async Task<InventoryResponse> CreateInventoryAsync(
            CreateInventoryRequest request,
            CancellationToken cancellationToken = default)
        {
            // A relief station can only have one inventory per level
            if (await _unitOfWork.Inventories.IsLevelExistsForStationAsync(
                    request.ReliefStationId, request.Level, cancellationToken: cancellationToken))
            {
                throw new InvalidOperationException(
                    $"Relief station already has a {request.Level} inventory.");
            }

            var inventory = new Inventory
            {
                InventoryId = Guid.NewGuid(),
                ReliefStationId = request.ReliefStationId,
                Level = request.Level,
                Status = request.Status
            };

            await _unitOfWork.Inventories.AddAsync(inventory);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Reload with navigation to get ReliefStation name
            var created = await _unitOfWork.Inventories.GetByIdWithStocksAsync(inventory.InventoryId, cancellationToken);
            return MapToResponse(created!);
        }

        /// <inheritdoc/>
        public async Task<InventoryDetailResponse> GetInventoryByIdAsync(
            Guid inventoryId,
            CancellationToken cancellationToken = default)
        {
            var inventory = await _unitOfWork.Inventories.GetByIdWithStocksAsync(inventoryId, cancellationToken);
            if (inventory is null)
            {
                throw new KeyNotFoundException($"Inventory '{inventoryId}' was not found.");
            }

            return MapToDetailResponse(inventory);
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<InventoryResponse>> GetAllInventoriesAsync(
            Guid? reliefStationId = null,
            CancellationToken cancellationToken = default)
        {
            var inventories = await _unitOfWork.Inventories.GetAllActiveAsync(reliefStationId, cancellationToken);
            return inventories.Select(MapToResponse).ToList();
        }

        /// <inheritdoc/>
        public async Task<InventoryResponse> UpdateInventoryAsync(
            Guid inventoryId,
            UpdateInventoryRequest request,
            CancellationToken cancellationToken = default)
        {
            var inventory = await _unitOfWork.Inventories.GetByIdWithStocksAsync(inventoryId, cancellationToken);
            if (inventory is null)
            {
                throw new KeyNotFoundException($"Inventory '{inventoryId}' was not found.");
            }

            // If level changes, check there's no conflict for this station
            if (inventory.Level != request.Level &&
                await _unitOfWork.Inventories.IsLevelExistsForStationAsync(
                    inventory.ReliefStationId, request.Level, excludeId: inventoryId, cancellationToken: cancellationToken))
            {
                throw new InvalidOperationException(
                    $"Relief station already has a {request.Level} inventory.");
            }

            inventory.Level = request.Level;
            inventory.Status = request.Status;

            await _unitOfWork.Inventories.UpdateAsync(inventory);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return MapToResponse(inventory);
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteInventoryAsync(
            Guid inventoryId,
            CancellationToken cancellationToken = default)
        {
            var inventory = await _unitOfWork.Inventories.GetByIdAsync(inventoryId);
            if (inventory is null || inventory.Status == EntityStatus.Deleted)
            {
                throw new KeyNotFoundException($"Inventory '{inventoryId}' was not found.");
            }

            // Soft-delete
            inventory.Status = EntityStatus.Deleted;
            await _unitOfWork.Inventories.UpdateAsync(inventory);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true;
        }

        // ═══════════════════════════════════════════════════════════
        //  STOCK MANAGEMENT
        // ═══════════════════════════════════════════════════════════

        /// <inheritdoc/>
        public async Task<InventoryStockResponse> AddStockItemAsync(
            Guid inventoryId,
            AddStockItemRequest request,
            CancellationToken cancellationToken = default)
        {
            // Validate inventory exists and is active
            var inventory = await _unitOfWork.Inventories.GetByIdAsync(inventoryId);
            if (inventory is null || inventory.Status == EntityStatus.Deleted)
            {
                throw new KeyNotFoundException($"Inventory '{inventoryId}' was not found.");
            }

            if (inventory.Status == EntityStatus.Inactive)
            {
                throw new InvalidOperationException("Cannot add stock to an inactive inventory.");
            }

            // Validate supply item exists
            var supplyItem = await _unitOfWork.SupplyItems.GetByIdAsync(request.SupplyItemId);
            if (supplyItem is null)
            {
                throw new KeyNotFoundException($"Supply item '{request.SupplyItemId}' was not found.");
            }

            // Enforce unique (InventoryId, SupplyItemId)
            if (await _unitOfWork.InventoryStocks.IsSupplyItemExistsInInventoryAsync(
                    inventoryId, request.SupplyItemId, cancellationToken: cancellationToken))
            {
                throw new InvalidOperationException(
                    $"Supply item '{supplyItem.Name}' is already registered in this inventory.");
            }

            // Validate stock levels
            if (request.MinimumStockLevel > request.MaximumStockLevel)
            {
                throw new InvalidOperationException("MinimumStockLevel cannot exceed MaximumStockLevel.");
            }

            var stock = new InventoryStock
            {
                InventoryStockId = Guid.NewGuid(),
                InventoryId = inventoryId,
                SupplyItemId = request.SupplyItemId,
                CurrentQuantity = request.CurrentQuantity,
                MinimumStockLevel = request.MinimumStockLevel,
                MaximumStockLevel = request.MaximumStockLevel
            };

            await _unitOfWork.InventoryStocks.AddAsync(stock);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Reload with navigation for response
            var createdStock = await _unitOfWork.InventoryStocks.GetByIdWithDetailsAsync(stock.InventoryStockId, cancellationToken);
            return MapToStockResponse(createdStock!);
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<InventoryStockResponse>> GetStocksByInventoryIdAsync(
            Guid inventoryId,
            CancellationToken cancellationToken = default)
        {
            // Ensure inventory exists
            if (!await _unitOfWork.Inventories.ExistsAsync(inventoryId))
            {
                throw new KeyNotFoundException($"Inventory '{inventoryId}' was not found.");
            }

            var stocks = await _unitOfWork.InventoryStocks.GetByInventoryIdAsync(inventoryId, cancellationToken);
            return stocks.Select(MapToStockResponse).ToList();
        }

        /// <inheritdoc/>
        public async Task<InventoryStockResponse> UpdateStockItemAsync(
            Guid stockId,
            UpdateStockItemRequest request,
            CancellationToken cancellationToken = default)
        {
            var stock = await _unitOfWork.InventoryStocks.GetByIdWithDetailsAsync(stockId, cancellationToken);
            if (stock is null)
            {
                throw new KeyNotFoundException($"Stock entry '{stockId}' was not found.");
            }

            if (request.MinimumStockLevel > request.MaximumStockLevel)
            {
                throw new InvalidOperationException("MinimumStockLevel cannot exceed MaximumStockLevel.");
            }

            stock.MinimumStockLevel = request.MinimumStockLevel;
            stock.MaximumStockLevel = request.MaximumStockLevel;

            await _unitOfWork.InventoryStocks.UpdateAsync(stock);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return MapToStockResponse(stock);
        }

        /// <inheritdoc/>
        public async Task<bool> RemoveStockItemAsync(
            Guid stockId,
            CancellationToken cancellationToken = default)
        {
            var stock = await _unitOfWork.InventoryStocks.GetByIdAsync(stockId);
            if (stock is null)
            {
                throw new KeyNotFoundException($"Stock entry '{stockId}' was not found.");
            }

            await _unitOfWork.InventoryStocks.DeleteAsync(stock);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true;
        }

        // ═══════════════════════════════════════════════════════════
        //  PRIVATE HELPERS
        // ═══════════════════════════════════════════════════════════

        private static InventoryResponse MapToResponse(Inventory inv) => new()
        {
            InventoryId = inv.InventoryId,
            ReliefStationId = inv.ReliefStationId,
            ReliefStationName = inv.ReliefStation?.Name ?? string.Empty,
            Level = inv.Level,
            Status = inv.Status,
            TotalStockSlots = inv.InventoryItems.Count,
            CriticalCount = inv.InventoryItems.Count(s => s.InventoryStatus == InventoryStatus.Critical)
        };

        private static InventoryDetailResponse MapToDetailResponse(Inventory inv) => new()
        {
            InventoryId = inv.InventoryId,
            ReliefStationId = inv.ReliefStationId,
            ReliefStationName = inv.ReliefStation?.Name ?? string.Empty,
            Level = inv.Level,
            Status = inv.Status,
            TotalStockSlots = inv.InventoryItems.Count,
            CriticalCount = inv.InventoryItems.Count(s => s.InventoryStatus == InventoryStatus.Critical),
            Stocks = inv.InventoryItems.Select(MapToStockResponse).ToList()
        };

        private static InventoryStockResponse MapToStockResponse(InventoryStock s) => new()
        {
            InventoryStockId = s.InventoryStockId,
            InventoryId = s.InventoryId,
            SupplyItemId = s.SupplyItemId,
            SupplyItemName = s.SupplyItem?.Name ?? string.Empty,
            SupplyItemUnit = s.SupplyItem?.Unit ?? string.Empty,
            SupplyItemCategory = s.SupplyItem?.Category ?? SupplyCategory.Khac,
            CurrentQuantity = s.CurrentQuantity,
            MinimumStockLevel = s.MinimumStockLevel,
            MaximumStockLevel = s.MaximumStockLevel,
            StockStatus = s.InventoryStatus
        };
    }
}
