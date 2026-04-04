using ReliefManagementSystem.Application.Common.Interface;
using ReliefManagementSystem.Application.Features.InventoryTransaction.DTOs.Request;
using ReliefManagementSystem.Application.Features.SupplyAllocation.DTOs.Request;
using ReliefManagementSystem.Application.Features.SupplyAllocation.DTOs.Response;
using ReliefManagementSystem.Application.Interface;
using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Application.Services
{
    /// <summary>
    /// Handles business logic for the SupplyAllocation workflow.
    /// Stock changes are atomic: deducted on Approve, returned on Cancel-after-Approve.
    /// </summary>
    public class SupplyAllocationService : ISupplyAllocationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IInventoryTransactionService _inventoryTransactionService;

        public SupplyAllocationService(
            IUnitOfWork unitOfWork,
            IInventoryTransactionService inventoryTransactionService)
        {
            _unitOfWork = unitOfWork;
            _inventoryTransactionService = inventoryTransactionService;
        }

        // ═══════════════════════════════════════════════════
        //  CREATE
        // ═══════════════════════════════════════════════════

        /// <inheritdoc/>
        public async Task<SupplyAllocationResponse> CreateAsync(
            CreateSupplyAllocationRequest request,
            CancellationToken cancellationToken = default)
        {
            // 1. Validate source inventory exists and is active
            var inventory = await _unitOfWork.Inventories.GetByIdAsync(request.SourceInventoryId);
            if (inventory is null || inventory.Status == EntityStatus.Deleted)
                throw new KeyNotFoundException($"Inventory '{request.SourceInventoryId}' was not found.");
            if (inventory.Status == EntityStatus.Inactive)
                throw new InvalidOperationException("Cannot allocate from an inactive inventory.");

            // 2. Validate campaign exists
            var campaign = await _unitOfWork.Campaigns.GetWithStationsAsync(request.CampaignId, cancellationToken);
            if (campaign is null)
                throw new KeyNotFoundException($"Campaign '{request.CampaignId}' was not found.");

            if (campaign.Type == CampaignType.Relief)
            {
                var activeStation = campaign.CampaignStations.FirstOrDefault(cs => cs.IsActive);
                if (activeStation is null)
                    throw new InvalidOperationException("Relief campaign phải có relief station active trước khi tạo supply allocation.");

                if (inventory.ReliefStationId != activeStation.ReliefStationId)
                    throw new InvalidOperationException("Relief campaign chỉ được cấp phát từ inventory thuộc station đang gắn với campaign.");
            }

            // 3. Reject empty items list
            if (request.Items is null || request.Items.Count == 0)
                throw new InvalidOperationException("At least one item is required.");

            // 4. Check duplicate SupplyItemId within request
            var duplicates = request.Items
                .GroupBy(i => i.SupplyItemId)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (duplicates.Count != 0)
                throw new InvalidOperationException(
                    $"Duplicate supply items in request: {string.Join(", ", duplicates)}");

            // 5. Validate each supply item is registered in the source inventory
            var stocks = await _unitOfWork.InventoryStocks
                .GetByInventoryIdAsync(request.SourceInventoryId, cancellationToken);

            foreach (var itemReq in request.Items)
            {
                var stock = stocks.FirstOrDefault(s => s.SupplyItemId == itemReq.SupplyItemId);
                if (stock is null)
                    throw new InvalidOperationException(
                        $"Supply item '{itemReq.SupplyItemId}' is not registered in this inventory.");
            }

            // 6. Create allocation in Pending status (no stock change yet)
            var allocation = new Domain.Entities.SupplyAllocation
            {
                AllocationId = Guid.NewGuid(),
                CampaignId = request.CampaignId,
                SourceInventoryId = request.SourceInventoryId,
                AllocatedAt = DateTime.UtcNow,
                Status = SupplyAllocationStatus.Pending,
                Items = request.Items.Select(i => new Domain.Entities.SupplyAllocationItem
                {
                    AllocationItemId = Guid.NewGuid(),
                    SupplyItemId = i.SupplyItemId,
                    Quantity = i.Quantity
                }).ToList()
            };

            await _unitOfWork.SupplyAllocations.AddAsync(allocation);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var saved = await _unitOfWork.SupplyAllocations
                .GetByIdWithDetailsAsync(allocation.AllocationId, cancellationToken);
            return MapToResponse(saved!);
        }

        // ═══════════════════════════════════════════════════
        //  GET
        // ═══════════════════════════════════════════════════

        /// <inheritdoc/>
        public async Task<SupplyAllocationResponse> GetByIdAsync(
            Guid allocationId,
            CancellationToken cancellationToken = default)
        {
            var allocation = await _unitOfWork.SupplyAllocations
                .GetByIdWithDetailsAsync(allocationId, cancellationToken);
            if (allocation is null)
                throw new KeyNotFoundException($"Supply allocation '{allocationId}' was not found.");

            return MapToResponse(allocation);
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<SupplyAllocationSummaryResponse>> GetByCampaignAsync(
            Guid campaignId,
            CancellationToken cancellationToken = default)
        {
            if (!await _unitOfWork.Campaigns.ExistsAsync(campaignId))
                throw new KeyNotFoundException($"Campaign '{campaignId}' was not found.");

            var allocations = await _unitOfWork.SupplyAllocations
                .GetByCampaignIdAsync(campaignId, cancellationToken);
            return allocations.Select(MapToSummary).ToList();
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<SupplyAllocationSummaryResponse>> GetByInventoryAsync(
            Guid inventoryId,
            CancellationToken cancellationToken = default)
        {
            if (!await _unitOfWork.Inventories.ExistsAsync(inventoryId))
                throw new KeyNotFoundException($"Inventory '{inventoryId}' was not found.");

            var allocations = await _unitOfWork.SupplyAllocations
                .GetByInventoryIdAsync(inventoryId, cancellationToken);
            return allocations.Select(MapToSummary).ToList();
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<SupplyAllocationSummaryResponse>> GetByStatusAsync(
            SupplyAllocationStatus status,
            CancellationToken cancellationToken = default)
        {
            var allocations = await _unitOfWork.SupplyAllocations
                .GetByStatusAsync(status, cancellationToken);
            return allocations.Select(MapToSummary).ToList();
        }

        // ═══════════════════════════════════════════════════
        //  STATUS TRANSITION (core workflow)
        // ═══════════════════════════════════════════════════

        /// <inheritdoc/>
        public async Task<SupplyAllocationResponse> UpdateStatusAsync(
            Guid allocationId,
            UpdateAllocationStatusRequest request,
            CancellationToken cancellationToken = default)
        {
            // Load with items (needed for stock calculations)
            var allocation = await _unitOfWork.SupplyAllocations
                .GetByIdWithDetailsAsync(allocationId, cancellationToken);
            if (allocation is null)
                throw new KeyNotFoundException($"Supply allocation '{allocationId}' was not found.");

            // Validate legal transition
            ValidateTransition(allocation.Status, request.Status);

            // ── Pending → Approved: deduct stock ───────────────
            if (request.Status == SupplyAllocationStatus.Approved)
            {
                await _unitOfWork.BeginTransactionAsync(cancellationToken);
                try
                {
                    var transaction = await _inventoryTransactionService.CreateTransactionAsync(new CreateTransactionRequest
                    {
                        InventoryId = allocation.SourceInventoryId,
                        Type = TransactionType.Export,
                        Reason = TransactionReason.CampaignAllocation,
                        Notes = $"Supply allocation approval: {allocation.AllocationId}",
                        Items = allocation.Items.Select(item => new TransactionItemRequest
                        {
                            SupplyItemId = item.SupplyItemId,
                            Quantity = item.Quantity,
                            Notes = $"Allocation {allocation.AllocationId}"
                        }).ToList()
                    }, autoSave: false, cancellationToken);

                    allocation.InventoryTransactionId = transaction.TransactionId;
                    allocation.Status = request.Status;
                    await _unitOfWork.SupplyAllocations.UpdateAsync(allocation);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                    await _unitOfWork.CommitTransactionAsync(cancellationToken);
                }
                catch
                {
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    throw;
                }

                var approved = await _unitOfWork.SupplyAllocations
                    .GetByIdWithDetailsAsync(allocationId, cancellationToken);
                return MapToResponse(approved!);
            }

            // ── Approved → Cancelled: return stock ─────────────
            if (allocation.Status == SupplyAllocationStatus.Approved &&
                request.Status == SupplyAllocationStatus.Cancelled)
            {
                await _unitOfWork.BeginTransactionAsync(cancellationToken);
                try
                {
                    await _inventoryTransactionService.CreateTransactionAsync(new CreateTransactionRequest
                    {
                        InventoryId = allocation.SourceInventoryId,
                        Type = TransactionType.Import,
                        Reason = TransactionReason.CampaignAllocation,
                        Notes = $"Supply allocation cancellation: {allocation.AllocationId}",
                        Items = allocation.Items.Select(item => new TransactionItemRequest
                        {
                            SupplyItemId = item.SupplyItemId,
                            Quantity = item.Quantity,
                            Notes = $"Allocation reversal {allocation.AllocationId}"
                        }).ToList()
                    }, autoSave: false, cancellationToken);

                    allocation.Status = request.Status;
                    await _unitOfWork.SupplyAllocations.UpdateAsync(allocation);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                    await _unitOfWork.CommitTransactionAsync(cancellationToken);
                }
                catch
                {
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    throw;
                }

                var cancelled = await _unitOfWork.SupplyAllocations
                    .GetByIdWithDetailsAsync(allocationId, cancellationToken);
                return MapToResponse(cancelled!);
            }

            allocation.Status = request.Status;
            await _unitOfWork.SupplyAllocations.UpdateAsync(allocation);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Reload for fresh navigation properties
            var updated = await _unitOfWork.SupplyAllocations
                .GetByIdWithDetailsAsync(allocationId, cancellationToken);
            return MapToResponse(updated!);
        }

        // ═══════════════════════════════════════════════════
        //  PRIVATE HELPERS
        // ═══════════════════════════════════════════════════

        /// <summary>
        /// Enforces valid one-direction workflow transitions.
        /// </summary>
        private static void ValidateTransition(SupplyAllocationStatus current, SupplyAllocationStatus next)
        {
            bool valid = (current, next) switch
            {
                (SupplyAllocationStatus.Pending, SupplyAllocationStatus.Approved) => true,
                (SupplyAllocationStatus.Pending, SupplyAllocationStatus.Cancelled) => true,
                (SupplyAllocationStatus.Approved, SupplyAllocationStatus.Delivered) => true,
                (SupplyAllocationStatus.Approved, SupplyAllocationStatus.Cancelled) => true,
                _ => false
            };

            if (!valid)
                throw new InvalidOperationException(
                    $"Cannot transition allocation from '{current}' to '{next}'. " +
                    $"Valid transitions: Pending→Approved, Pending→Cancelled, Approved→Delivered, Approved→Cancelled.");
        }

        private static SupplyAllocationResponse MapToResponse(Domain.Entities.SupplyAllocation a) => new()
        {
            AllocationId = a.AllocationId,
            CampaignId = a.CampaignId,
            SourceInventoryId = a.SourceInventoryId,
            SourceInventoryName = a.SourceInventory?.ReliefStation?.Name
                ?? a.SourceInventoryId.ToString(),
            ReliefStationName = a.SourceInventory?.ReliefStation?.Name ?? string.Empty,
            InventoryTransactionId = a.InventoryTransactionId,
            Status = a.Status,
            AllocatedAt = a.AllocatedAt,
            Items = a.Items.Select(i => new AllocationItemResponse
            {
                AllocationItemId = i.AllocationItemId,
                SupplyItemId = i.SupplyItemId,
                SupplyItemName = i.SupplyItem?.Name ?? string.Empty,
                SupplyItemUnit = i.SupplyItem?.Unit ?? string.Empty,
                Quantity = i.Quantity
            }).ToList()
        };

        private static SupplyAllocationSummaryResponse MapToSummary(Domain.Entities.SupplyAllocation a) => new()
        {
            AllocationId = a.AllocationId,
            CampaignId = a.CampaignId,
            SourceInventoryId = a.SourceInventoryId,
            SourceInventoryName = a.SourceInventory?.ReliefStation?.Name
                ?? a.SourceInventoryId.ToString(),
            Status = a.Status,
            TotalItems = a.Items.Count,
            AllocatedAt = a.AllocatedAt,
            Items = a.Items.Select(i => new AllocationItemResponse
            {
                AllocationItemId = i.AllocationItemId,
                SupplyItemId = i.SupplyItemId,
                SupplyItemName = i.SupplyItem?.Name ?? string.Empty,
                SupplyItemUnit = i.SupplyItem?.Unit ?? string.Empty,
                Quantity = i.Quantity
            }).ToList()
        };
    }
}
