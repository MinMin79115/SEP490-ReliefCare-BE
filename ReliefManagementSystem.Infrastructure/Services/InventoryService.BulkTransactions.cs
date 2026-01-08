using Microsoft.AspNetCore.Identity;
using ReliefManagementSystem.Application.Common.Interface;
using ReliefManagementSystem.Application.Features.Inventory;
using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Infrastructure.Services
{
    public partial class InventoryService
    {
        #region Bulk Transactions

        public async Task<BulkTransactionResponse> BulkImportAsync(
            BulkImportRequest request,
            Guid userId,
            CancellationToken cancellationToken)
        {
            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {
                // 1. Validate all items first
                var validationErrors = new List<string>();
                var itemsToProcess = new List<(InventoryItem item, decimal quantity, string? notes)>();

                foreach (var itemRequest in request.Items)
                {
                    var inventoryItem = await _inventoryItemRepository.GetByIdAsync(
                        itemRequest.InventoryItemId, cancellationToken);

                    if (inventoryItem == null)
                    {
                        validationErrors.Add($"Item {itemRequest.InventoryItemId} not found");
                        continue;
                    }

                    if (itemRequest.Quantity <= 0)
                    {
                        validationErrors.Add($"{inventoryItem.Name}: Quantity must be greater than 0");
                        continue;
                    }

                    var newQuantity = inventoryItem.CurrentQuantity + itemRequest.Quantity;
                    if (newQuantity > inventoryItem.MaxCapacity)
                    {
                        validationErrors.Add(
                            $"{inventoryItem.Name}: Exceeds capacity " +
                            $"(Current: {inventoryItem.CurrentQuantity}, " +
                            $"Adding: {itemRequest.Quantity}, " +
                            $"Max: {inventoryItem.MaxCapacity})");
                        continue;
                    }

                    itemsToProcess.Add((inventoryItem, itemRequest.Quantity, itemRequest.Notes));
                }

                if (validationErrors.Any())
                    throw new Exception(string.Join("; ", validationErrors));

                // 2. Create batch
                var batchNumber = await _batchRepository.GenerateBatchNumberAsync("PN", cancellationToken);
                var batch = new ImportExportBatch
                {
                    BatchId = Guid.NewGuid(),
                    BatchNumber = batchNumber,
                    BatchType = TransactionType.Import,
                    CreatedBy = userId,
                    CreatedAt = DateTime.UtcNow,
                    Notes = request.Notes,
                    Status = BatchStatus.Confirmed
                };

                await _batchRepository.AddAsync(batch);

                // 3. Process each item
                var results = new List<ItemTransactionResult>();
                var transactions = new List<WarehouseTransaction>();

                foreach (var (item, quantity, notes) in itemsToProcess)
                {
                    var previousQty = item.CurrentQuantity;
                    item.CurrentQuantity += quantity;
                    item.Status = CalculateStatus(item.CurrentQuantity, item.MaxCapacity);
                    item.UpdatedAt = DateTime.UtcNow;

                    _inventoryItemRepository.Update(item);

                    // Create transaction record
                    var warehouseTransaction = new WarehouseTransaction
                    {
                        TransactionId = Guid.NewGuid(),
                        BatchId = batch.BatchId,
                        InventoryItemId = item.InventoryItemId,
                        Quantity = quantity,
                        Notes = notes
                    };

                    transactions.Add(warehouseTransaction);

                    results.Add(new ItemTransactionResult
                    {
                        ItemId = item.InventoryItemId,
                        ItemName = item.Name,
                        PreviousQuantity = previousQty,
                        TransactionQuantity = quantity,
                        CurrentQuantity = item.CurrentQuantity,
                        Status = item.Status,
                        StatusText = GetStatusText(item.Status)
                    });
                }

                await _transactionRepository.BulkAddAsync(transactions, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                var creator = await _userManager.FindByIdAsync(userId.ToString());

                return new BulkTransactionResponse
                {
                    BatchId = batch.BatchId,
                    BatchNumber = batch.BatchNumber,
                    BatchType = TransactionType.Import,
                    TotalItems = results.Count,
                    CreatedAt = batch.CreatedAt,
                    CreatedBy = creator?.DisplayName ?? creator?.UserName ?? "Unknown",
                    Notes = batch.Notes,
                    Items = results
                };
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                throw;
            }
        }

        public async Task<BulkTransactionResponse> BulkExportAsync(
            BulkExportRequest request,
            Guid userId,
            CancellationToken cancellationToken)
        {
            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {
                // 1. Validate all items first
                var validationErrors = new List<string>();
                var itemsToProcess = new List<(InventoryItem item, decimal quantity, string? notes)>();

                foreach (var itemRequest in request.Items)
                {
                    var inventoryItem = await _inventoryItemRepository.GetByIdAsync(
                        itemRequest.InventoryItemId, cancellationToken);

                    if (inventoryItem == null)
                    {
                        validationErrors.Add($"Item {itemRequest.InventoryItemId} not found");
                        continue;
                    }

                    if (itemRequest.Quantity <= 0)
                    {
                        validationErrors.Add($"{inventoryItem.Name}: Quantity must be greater than 0");
                        continue;
                    }

                    if (inventoryItem.CurrentQuantity < itemRequest.Quantity)
                    {
                        validationErrors.Add(
                            $"{inventoryItem.Name}: Insufficient stock " +
                            $"(Available: {inventoryItem.CurrentQuantity}, " +
                            $"Requested: {itemRequest.Quantity})");
                        continue;
                    }

                    itemsToProcess.Add((inventoryItem, itemRequest.Quantity, itemRequest.Notes));
                }

                if (validationErrors.Any())
                    throw new Exception(string.Join("; ", validationErrors));

                // 2. Create batch
                var batchNumber = await _batchRepository.GenerateBatchNumberAsync("PX", cancellationToken);
                var batch = new ImportExportBatch
                {
                    BatchId = Guid.NewGuid(),
                    BatchNumber = batchNumber,
                    BatchType = TransactionType.Export,
                    CreatedBy = userId,
                    CreatedAt = DateTime.UtcNow,
                    Notes = request.Notes,
                    RecipientInfo = request.RecipientInfo,
                    Status = BatchStatus.Confirmed
                };

                await _batchRepository.AddAsync(batch);

                // 3. Process each item
                var results = new List<ItemTransactionResult>();
                var transactions = new List<WarehouseTransaction>();

                foreach (var (item, quantity, notes) in itemsToProcess)
                {
                    var previousQty = item.CurrentQuantity;
                    item.CurrentQuantity -= quantity;
                    item.Status = CalculateStatus(item.CurrentQuantity, item.MaxCapacity);
                    item.UpdatedAt = DateTime.UtcNow;

                    _inventoryItemRepository.Update(item);

                    // Create transaction record
                    var warehouseTransaction = new WarehouseTransaction
                    {
                        TransactionId = Guid.NewGuid(),
                        BatchId = batch.BatchId,
                        InventoryItemId = item.InventoryItemId,
                        Quantity = quantity,
                        Notes = notes
                    };

                    transactions.Add(warehouseTransaction);

                    results.Add(new ItemTransactionResult
                    {
                        ItemId = item.InventoryItemId,
                        ItemName = item.Name,
                        PreviousQuantity = previousQty,
                        TransactionQuantity = quantity,
                        CurrentQuantity = item.CurrentQuantity,
                        Status = item.Status,
                        StatusText = GetStatusText(item.Status)
                    });
                }

                await _transactionRepository.BulkAddAsync(transactions, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                var creator = await _userManager.FindByIdAsync(userId.ToString());

                return new BulkTransactionResponse
                {
                    BatchId = batch.BatchId,
                    BatchNumber = batch.BatchNumber,
                    BatchType = TransactionType.Export,
                    TotalItems = results.Count,
                    CreatedAt = batch.CreatedAt,
                    CreatedBy = creator?.DisplayName ?? creator?.UserName ?? "Unknown",
                    Notes = batch.Notes,
                    RecipientInfo = batch.RecipientInfo,
                    Items = results
                };
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                throw;
            }
        }

        #endregion

        #region Batches

        public async Task<List<BatchDto>> GetBatchesAsync(
            TransactionType? type,
            int page,
            int pageSize,
            CancellationToken cancellationToken)
        {
            var batches = type.HasValue
                ? await _batchRepository.GetByTypeAsync(type.Value, page, pageSize, cancellationToken)
                : await _batchRepository.GetAllWithDetailsAsync(page, pageSize, cancellationToken);

            return batches.Select(b => new BatchDto
            {
                BatchId = b.BatchId,
                BatchNumber = b.BatchNumber,
                BatchType = b.BatchType,
                BatchTypeText = b.BatchType == TransactionType.Import ? "Nhập kho" : "Xuất kho",
                TotalItems = b.Transactions.Count,
                CreatedAt = b.CreatedAt,
                CreatedBy = b.Creator.DisplayName ?? b.Creator.UserName ?? "Unknown",
                Notes = b.Notes,
                RecipientInfo = b.RecipientInfo,
                Status = b.Status
            }).ToList();
        }

        public async Task<BatchDetailDto?> GetBatchDetailAsync(
            Guid batchId,
            CancellationToken cancellationToken)
        {
            var batch = await _batchRepository.GetByIdWithDetailsAsync(batchId, cancellationToken);
            if (batch == null)
                return null;

            return new BatchDetailDto
            {
                BatchId = batch.BatchId,
                BatchNumber = batch.BatchNumber,
                BatchType = batch.BatchType,
                BatchTypeText = batch.BatchType == TransactionType.Import ? "Nhập kho" : "Xuất kho",
                TotalItems = batch.Transactions.Count,
                CreatedAt = batch.CreatedAt,
                CreatedBy = batch.Creator.DisplayName ?? batch.Creator.UserName ?? "Unknown",
                Notes = batch.Notes,
                RecipientInfo = batch.RecipientInfo,
                Status = batch.Status,
                Items = batch.Transactions.Select(t => new TransactionItemDto
                {
                    TransactionId = t.TransactionId,
                    ItemName = t.InventoryItem.Name,
                    ItemCode = t.InventoryItem.Code,
                    Quantity = t.Quantity,
                    Unit = t.InventoryItem.Unit,
                    Notes = t.Notes
                }).ToList()
            };
        }

        #endregion
    }
}
