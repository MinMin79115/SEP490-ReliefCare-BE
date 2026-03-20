using ReliefManagementSystem.Application.Common.Interface;
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

        public SupplyAllocationService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
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
            if (!await _unitOfWork.Campaigns.ExistsAsync(request.CampaignId))
                throw new KeyNotFoundException($"Campaign '{request.CampaignId}' was not found.");

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

            var stocks = await _unitOfWork.InventoryStocks
                .GetByInventoryIdAsync(allocation.SourceInventoryId, cancellationToken);

            // ── Pending → Approved: deduct stock ───────────────
            if (request.Status == SupplyAllocationStatus.Approved)
            {
                var stockUpdates = new List<(Domain.Entities.InventoryStock Stock, int NewQty)>();

                foreach (var item in allocation.Items)
                {
                    var stock = stocks.FirstOrDefault(s => s.SupplyItemId == item.SupplyItemId)
                        ?? throw new InvalidOperationException(
                            $"Supply item '{item.SupplyItemId}' is no longer registered in this inventory.");

                    if (stock.CurrentQuantity < item.Quantity)
                        throw new InvalidOperationException(
                            $"Insufficient stock for '{item.SupplyItem?.Name ?? item.SupplyItemId.ToString()}'. " +
                            $"Available: {stock.CurrentQuantity}, Requested: {item.Quantity}.");

                    stockUpdates.Add((stock, stock.CurrentQuantity - item.Quantity));
                }

                foreach (var (stock, newQty) in stockUpdates)
                {
                    stock.CurrentQuantity = newQty;
                    await _unitOfWork.InventoryStocks.UpdateAsync(stock);
                }
            }

            // ── Approved → Cancelled: return stock ─────────────
            if (allocation.Status == SupplyAllocationStatus.Approved &&
                request.Status == SupplyAllocationStatus.Cancelled)
            {
                foreach (var item in allocation.Items)
                {
                    var stock = stocks.FirstOrDefault(s => s.SupplyItemId == item.SupplyItemId);
                    if (stock is not null)
                    {
                        stock.CurrentQuantity += item.Quantity;
                        await _unitOfWork.InventoryStocks.UpdateAsync(stock);
                    }
                }
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
            AllocatedAt = a.AllocatedAt
        };
    }
}
