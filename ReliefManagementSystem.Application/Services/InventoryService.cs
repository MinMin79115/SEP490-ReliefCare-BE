using Microsoft.EntityFrameworkCore;
using ReliefManagementSystem.Application.Common.Interface;
using ReliefManagementSystem.Application.Common.Models;
using ReliefManagementSystem.Application.Features.Inventory.DTOs;
using ReliefManagementSystem.Application.Interface;
using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Application.Services
{
    public class InventoryService : IInventoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public InventoryService(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<SupplyItemDto> CreateSupplyItemAsync(
            CreateSupplyItemRequest request,
            CancellationToken cancellationToken = default)
        {
            // Validate MinimumStockLevel <= MaximumStockLevel
            if (request.MinimumStockLevel > request.MaximumStockLevel)
            {
                throw new InvalidOperationException(
                    $"MinimumStockLevel ({request.MinimumStockLevel}) cannot be greater than MaximumStockLevel ({request.MaximumStockLevel})");
            }

            var supplyItem = new SupplyItem
            {
                Name = request.Name,
                Description = request.Description,
                IconUrl = request.IconUrl,
                Category = request.Category,
                Unit = request.Unit,
                CurrentQuantity = request.CurrentQuantity,
                MinimumStockLevel = request.MinimumStockLevel,
                MaximumStockLevel = request.MaximumStockLevel,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.SupplyItems.AddAsync(supplyItem);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return MapToDto(supplyItem);
        }

        public async Task<SupplyItemDto> UpdateSupplyItemAsync(
            Guid id,
            UpdateSupplyItemRequest request,
            CancellationToken cancellationToken = default)
        {
            // Validate MinimumStockLevel <= MaximumStockLevel
            if (request.MinimumStockLevel > request.MaximumStockLevel)
            {
                throw new InvalidOperationException(
                    $"MinimumStockLevel ({request.MinimumStockLevel}) cannot be greater than MaximumStockLevel ({request.MaximumStockLevel})");
            }

            var supplyItem = await _unitOfWork.SupplyItems.GetByIdAsync(id);

            if (supplyItem == null)
                throw new KeyNotFoundException($"Supply item with ID {id} not found");

            supplyItem.Name = request.Name;
            supplyItem.Description = request.Description;
            supplyItem.IconUrl = request.IconUrl;
            supplyItem.Category = request.Category;
            supplyItem.Unit = request.Unit;
            supplyItem.MinimumStockLevel = request.MinimumStockLevel;
            supplyItem.MaximumStockLevel = request.MaximumStockLevel;
            supplyItem.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.SupplyItems.UpdateAsync(supplyItem);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return MapToDto(supplyItem);
        }

        public async Task<bool> DeleteSupplyItemAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var supplyItem = await _unitOfWork.SupplyItems.GetByIdAsync(id);

            if (supplyItem == null)
                return false;

            await _unitOfWork.SupplyItems.DeleteAsync(supplyItem);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true;
        }

        public async Task<SupplyItemDto?> GetSupplyItemByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var supplyItem = await _unitOfWork.SupplyItems.GetByIdAsync(id);

            return supplyItem == null ? null : MapToDto(supplyItem);
        }

        public async Task<Pagination<SupplyItemDto>> GetSupplyItemsAsync(
            SupplyCategory? category,
            InventoryStatus? status,
            string? search,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            // Use optimized repository method for database-level filtering
            var items = await _unitOfWork.SupplyItems.GetFilteredAsync(
                category,
                search,
                cancellationToken);

            // Filter by status (computed property) - must be done in-memory
            if (status.HasValue)
            {
                items = items.Where(s => s.Status == status.Value).ToList();
            }

            var totalItems = items.Count;
            var paginatedItems = items
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(MapToDto)
                .ToList();

            return new Pagination<SupplyItemDto>(paginatedItems, totalItems, page, pageSize);
        }

        public async Task<BulkTransactionResult> BulkImportAsync(
            BulkImportRequest request,
            CancellationToken cancellationToken = default)
        {
            // 1. Validate all items exist
            var itemIds = request.Items.Select(i => i.SupplyItemId).ToList();
            var supplyItems = await _unitOfWork.SupplyItems.GetByIdsAsync(itemIds, cancellationToken);

            if (supplyItems.Count != itemIds.Distinct().Count())
                throw new KeyNotFoundException("One or more supply items not found");

            // 2. Create transaction
            var transaction = new InventoryTransaction
            {
                TransactionCode = GenerateTransactionCode("IN"),
                Type = TransactionType.Import,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = _currentUserService.UserId,
                Notes = request.Notes,
                Items = new List<InventoryTransactionItem>()
            };

            // 3. Process each item
            foreach (var importItem in request.Items)
            {
                var supplyItem = supplyItems.First(s => s.SupplyItemId == importItem.SupplyItemId);

                // Update quantity
                supplyItem.CurrentQuantity += importItem.Quantity;
                supplyItem.UpdatedAt = DateTime.UtcNow;

                // Add transaction item
                transaction.Items.Add(new InventoryTransactionItem
                {
                    SupplyItemId = importItem.SupplyItemId,
                    Quantity = importItem.Quantity,
                    Notes = importItem.Notes
                });

                await _unitOfWork.SupplyItems.UpdateAsync(supplyItem);
            }

            // 4. Save transaction
            await _unitOfWork.InventoryTransactions.AddAsync(transaction);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new BulkTransactionResult
            {
                TransactionId = transaction.TransactionId,
                TransactionCode = transaction.TransactionCode,
                ItemsProcessed = transaction.Items.Count,
                Success = true,
                Message = $"Successfully imported {transaction.Items.Count} items"
            };
        }

        public async Task<BulkTransactionResult> BulkExportAsync(
            BulkExportRequest request,
            CancellationToken cancellationToken = default)
        {
            // 1. Validate all items exist
            var itemIds = request.Items.Select(i => i.SupplyItemId).ToList();
            var supplyItems = await _unitOfWork.SupplyItems.GetByIdsAsync(itemIds, cancellationToken);

            if (supplyItems.Count != itemIds.Distinct().Count())
                throw new KeyNotFoundException("One or more supply items not found");

            // 2. Check sufficient quantities
            var insufficientItems = new List<string>();
            foreach (var exportItem in request.Items)
            {
                var supplyItem = supplyItems.First(s => s.SupplyItemId == exportItem.SupplyItemId);

                if (supplyItem.CurrentQuantity < exportItem.Quantity)
                {
                    insufficientItems.Add(
                        $"{supplyItem.Name} (Available: {supplyItem.CurrentQuantity}, Requested: {exportItem.Quantity})");
                }
            }

            if (insufficientItems.Any())
            {
                throw new InvalidOperationException(
                    $"Insufficient quantity for: {string.Join(", ", insufficientItems)}");
            }

            // 3. Create transaction
            var transaction = new InventoryTransaction
            {
                TransactionCode = GenerateTransactionCode("OUT"),
                Type = TransactionType.Export,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = _currentUserService.UserId,
                Notes = string.IsNullOrWhiteSpace(request.RecipientInfo)
                    ? request.Notes
                    : $"{request.Notes} | Recipient: {request.RecipientInfo}",
                Items = new List<InventoryTransactionItem>()
            };

            // 4. Process each item
            foreach (var exportItem in request.Items)
            {
                var supplyItem = supplyItems.First(s => s.SupplyItemId == exportItem.SupplyItemId);

                // Decrease quantity
                supplyItem.CurrentQuantity -= exportItem.Quantity;
                supplyItem.UpdatedAt = DateTime.UtcNow;

                // Add transaction item
                transaction.Items.Add(new InventoryTransactionItem
                {
                    SupplyItemId = exportItem.SupplyItemId,
                    Quantity = exportItem.Quantity,
                    Notes = exportItem.Notes
                });

                await _unitOfWork.SupplyItems.UpdateAsync(supplyItem);
            }

            // 5. Save transaction
            await _unitOfWork.InventoryTransactions.AddAsync(transaction);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new BulkTransactionResult
            {
                TransactionId = transaction.TransactionId,
                TransactionCode = transaction.TransactionCode,
                ItemsProcessed = transaction.Items.Count,
                Success = true,
                Message = $"Successfully exported {transaction.Items.Count} items"
            };
        }

        private string GenerateTransactionCode(string prefix)
        {
            return $"{prefix}-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpper()}";
        }

        private SupplyItemDto MapToDto(SupplyItem item)
        {
            var percentageFull = item.MaximumStockLevel > 0
                ? (decimal)item.CurrentQuantity / item.MaximumStockLevel * 100
                : 0;

            return new SupplyItemDto
            {
                SupplyItemId = item.SupplyItemId,
                Name = item.Name,
                Description = item.Description,
                IconUrl = item.IconUrl,
                Category = item.Category,
                CategoryName = item.Category.ToString(),
                Unit = item.Unit,
                CurrentQuantity = item.CurrentQuantity,
                MinimumStockLevel = item.MinimumStockLevel,
                MaximumStockLevel = item.MaximumStockLevel,
                Status = item.Status,
                StatusName = item.Status.ToString(),
                PercentageFull = Math.Round(percentageFull, 2),
                CreatedAt = item.CreatedAt,
                UpdatedAt = item.UpdatedAt
            };
        }

        public async Task<Pagination<InventoryTransactionDto>> GetTransactionsAsync(
            TransactionType? type,
            DateTime? startDate,
            DateTime? endDate,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            var transactions = await _unitOfWork.InventoryTransactions.GetAllAsync();
            var query = transactions.AsEnumerable();

            // Filter by type
            if (type.HasValue)
            {
                query = query.Where(t => t.Type == type.Value);
            }

            // Filter by date range
            if (startDate.HasValue)
            {
                query = query.Where(t => t.CreatedAt >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(t => t.CreatedAt <= endDate.Value);
            }

            var totalItems = query.Count();
            var paginatedTransactions = query
                .OrderByDescending(t => t.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(t => new InventoryTransactionDto
                {
                    TransactionId = t.TransactionId,
                    TransactionCode = t.TransactionCode,
                    Type = t.Type,
                    TypeName = t.Type.ToString(),
                    CreatedAt = t.CreatedAt,
                    CreatedBy = t.CreatedBy,
                    CreatedByName = t.CreatedByUser?.DisplayName,
                    CreatedByEmail = t.CreatedByUser?.Email,
                    Notes = t.Notes,
                    ItemCount = t.Items.Count,
                    TotalQuantity = t.Items.Sum(i => i.Quantity)
                })
                .ToList();

            return new Pagination<InventoryTransactionDto>(paginatedTransactions, totalItems, page, pageSize);
        }

        public async Task<InventoryTransactionDetailDto?> GetTransactionByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var transaction = await _unitOfWork.InventoryTransactions.GetByIdWithItemsAsync(id, cancellationToken);

            if (transaction == null)
                return null;

            return new InventoryTransactionDetailDto
            {
                TransactionId = transaction.TransactionId,
                TransactionCode = transaction.TransactionCode,
                Type = transaction.Type,
                TypeName = transaction.Type.ToString(),
                CreatedAt = transaction.CreatedAt,
                CreatedBy = transaction.CreatedBy,
                CreatedByName = transaction.CreatedByUser?.DisplayName,
                CreatedByEmail = transaction.CreatedByUser?.Email,
                Notes = transaction.Notes,
                Items = transaction.Items.Select(i => new TransactionItemDetailDto
                {
                    TransactionItemId = i.TransactionItemId,
                    SupplyItemId = i.SupplyItemId,
                    SupplyItemName = i.SupplyItem.Name,
                    Unit = i.SupplyItem.Unit,
                    Quantity = i.Quantity,
                    Notes = i.Notes
                }).ToList()
            };
        }
    }
}
