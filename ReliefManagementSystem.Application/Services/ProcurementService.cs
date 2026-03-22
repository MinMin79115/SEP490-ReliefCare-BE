using ReliefManagementSystem.Application.Common.Interface;
using ReliefManagementSystem.Application.Features.InventoryTransaction.DTOs.Request;
using ReliefManagementSystem.Application.Features.Procurement.Dtos.Requests;
using ReliefManagementSystem.Application.Features.Procurement.Dtos.Responses;
using ReliefManagementSystem.Application.Interface;
using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Application.Services
{
    public class ProcurementService : IProcurementService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly IInventoryTransactionService _inventoryTransactionService;

        public ProcurementService(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            IInventoryTransactionService inventoryTransactionService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _inventoryTransactionService = inventoryTransactionService;
        }

        public async Task<ProcurementOrderResponse> CreateAsync(CreateProcurementOrderRequest request, CancellationToken cancellationToken = default)
        {
            var campaign = await _unitOfWork.Campaigns.GetWithGoalsAsync(request.CampaignId, cancellationToken)
                ?? throw new KeyNotFoundException($"Campaign '{request.CampaignId}' was not found.");

            if (campaign.Type != CampaignType.Fundraising && campaign.Type != CampaignType.Relief)
            {
                throw new InvalidOperationException("Chỉ Fundraising hoặc Relief campaign mới có thể tạo procurement order.");
            }

            var inventory = await _unitOfWork.Inventories.GetByIdAsync(request.DestinationInventoryId)
                ?? throw new KeyNotFoundException($"Inventory '{request.DestinationInventoryId}' was not found.");

            var estimatedCost = request.Items.Sum(i => i.Quantity * i.UnitCost);
            var availableBudget = campaign.BudgetTotal - campaign.BudgetSpent;
            if (estimatedCost > availableBudget)
            {
                throw new InvalidOperationException($"Ngân sách khả dụng không đủ. Available={availableBudget}, Estimated={estimatedCost}.");
            }

            var order = new ProcurementOrder
            {
                ProcurementOrderId = Guid.NewGuid(),
                CampaignId = request.CampaignId,
                DestinationInventoryId = request.DestinationInventoryId,
                OrderCode = await GenerateOrderCodeAsync(cancellationToken),
                Status = ProcurementStatus.Draft,
                TotalEstimatedCost = estimatedCost,
                SupplierName = request.SupplierName,
                SupplierContact = request.SupplierContact,
                Notes = request.Notes,
                CreatedBy = _currentUserService.UserId ?? Guid.Empty,
                CreatedAt = DateTime.UtcNow,
                Items = request.Items.Select(i => new ProcurementOrderItem
                {
                    ProcurementOrderItemId = Guid.NewGuid(),
                    SupplyItemId = i.SupplyItemId,
                    Quantity = i.Quantity,
                    UnitCost = i.UnitCost
                }).ToList()
            };

            await _unitOfWork.ProcurementOrders.AddAsync(order);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return await GetByIdAsync(order.ProcurementOrderId, cancellationToken);
        }

        public async Task<ProcurementOrderResponse> GetByIdAsync(Guid procurementOrderId, CancellationToken cancellationToken = default)
        {
            var order = await LoadOrderAsync(procurementOrderId, cancellationToken)
                ?? throw new KeyNotFoundException($"Procurement order '{procurementOrderId}' was not found.");
            return Map(order);
        }

        public async Task<IReadOnlyList<ProcurementOrderResponse>> GetByCampaignAsync(Guid campaignId, CancellationToken cancellationToken = default)
        {
            var orders = await _unitOfWork.ProcurementOrders.GetByCampaignAsync(campaignId, cancellationToken);

            return orders.Select(Map).ToList();
        }

        public async Task<ProcurementOrderResponse> ApproveAsync(Guid procurementOrderId, ApproveProcurementOrderRequest request, CancellationToken cancellationToken = default)
        {
            var order = await LoadOrderAsync(procurementOrderId, cancellationToken)
                ?? throw new KeyNotFoundException($"Procurement order '{procurementOrderId}' was not found.");

            if (order.Status != ProcurementStatus.Draft)
            {
                throw new InvalidOperationException("Chỉ procurement order ở trạng thái Draft mới được duyệt.");
            }

            var campaign = await _unitOfWork.Campaigns.GetWithGoalsAsync(order.CampaignId, cancellationToken)
                ?? throw new KeyNotFoundException($"Campaign '{order.CampaignId}' was not found.");

            var availableBudget = campaign.BudgetTotal - campaign.BudgetSpent;
            if (order.TotalEstimatedCost > availableBudget)
            {
                throw new InvalidOperationException("Ngân sách không đủ để duyệt procurement order.");
            }

            order.Status = ProcurementStatus.Approved;
            order.ApprovedBy = _currentUserService.UserId;
            order.ApprovedAt = DateTime.UtcNow;
            order.ApprovalNote = request.ApprovalNote;

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Map(order);
        }

        public async Task<ProcurementOrderResponse> ReceiveAsync(Guid procurementOrderId, ReceiveProcurementOrderRequest request, CancellationToken cancellationToken = default)
        {
            var order = await LoadOrderAsync(procurementOrderId, cancellationToken)
                ?? throw new KeyNotFoundException($"Procurement order '{procurementOrderId}' was not found.");

            if (order.Status != ProcurementStatus.Approved && order.Status != ProcurementStatus.Ordered)
            {
                throw new InvalidOperationException("Chỉ procurement order đã duyệt/đặt hàng mới được nhận.");
            }

            var campaign = await _unitOfWork.Campaigns.GetWithGoalsAsync(order.CampaignId, cancellationToken)
                ?? throw new KeyNotFoundException($"Campaign '{order.CampaignId}' was not found.");

            var requestedItemsBySupply = request.Items.ToDictionary(i => i.SupplyItemId);

            if (requestedItemsBySupply.Count != order.Items.Count)
            {
                throw new InvalidOperationException("Receive request phải khai báo đầy đủ tất cả items của procurement order.");
            }

            foreach (var orderItem in order.Items)
            {
                if (!requestedItemsBySupply.TryGetValue(orderItem.SupplyItemId, out var receiveItem))
                {
                    throw new InvalidOperationException($"Supply item '{orderItem.SupplyItemId}' chưa được khai báo trong receive request.");
                }

                if (receiveItem.ReceivedQuantity > orderItem.Quantity)
                {
                    throw new InvalidOperationException($"ReceivedQuantity của supply item '{orderItem.SupplyItemId}' không được vượt quá số lượng đặt mua.");
                }

                orderItem.ReceivedQuantity = receiveItem.ReceivedQuantity;
                orderItem.ActualUnitCost = receiveItem.ActualUnitCost;

                await EnsureInventoryStockExistsAsync(order.DestinationInventoryId, receiveItem.SupplyItemId, cancellationToken);
            }

            var actualCost = order.Items.Sum(i => (i.ReceivedQuantity ?? 0) * (i.ActualUnitCost ?? i.UnitCost));
            var nextBudgetSpent = campaign.BudgetSpent + actualCost;
            if (nextBudgetSpent > campaign.BudgetTotal)
            {
                throw new InvalidOperationException("Không thể nhận hàng vì chi tiêu vượt quá BudgetTotal của campaign.");
            }

            await _unitOfWork.BeginTransactionAsync(cancellationToken);
            try
            {
                var transaction = await _inventoryTransactionService.CreateTransactionAsync(new CreateTransactionRequest
                {
                    InventoryId = order.DestinationInventoryId,
                    Type = TransactionType.Import,
                    Reason = TransactionReason.Procurement,
                    Notes = $"Procurement receive: {order.OrderCode}",
                    Items = order.Items.Select(i => new TransactionItemRequest
                    {
                        SupplyItemId = i.SupplyItemId,
                        Quantity = i.ReceivedQuantity ?? 0,
                        Notes = $"Procurement order {order.OrderCode}"
                    }).Where(i => i.Quantity > 0).ToList()
                }, autoSave: false, cancellationToken);

                order.InventoryTransactionId = transaction.TransactionId;
                order.TotalActualCost = actualCost;
                order.Status = ProcurementStatus.Received;
                order.ReceivedBy = _currentUserService.UserId;
                order.ReceivedAt = DateTime.UtcNow;
                order.ReceiveNote = request.ReceiveNote;

                campaign.BudgetSpent = nextBudgetSpent;

                await _unitOfWork.Campaigns.UpdateAsync(campaign);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await _unitOfWork.CommitTransactionAsync(cancellationToken);
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                throw;
            }

            return Map(order);
        }

        public async Task<ProcurementOrderResponse> CancelAsync(Guid procurementOrderId, CancellationToken cancellationToken = default)
        {
            var order = await LoadOrderAsync(procurementOrderId, cancellationToken)
                ?? throw new KeyNotFoundException($"Procurement order '{procurementOrderId}' was not found.");

            if (order.Status == ProcurementStatus.Received)
            {
                throw new InvalidOperationException("Không thể huỷ procurement order đã nhận hàng.");
            }

            order.Status = ProcurementStatus.Cancelled;
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Map(order);
        }

        private async Task<ProcurementOrder?> LoadOrderAsync(Guid procurementOrderId, CancellationToken cancellationToken)
        {
            return await _unitOfWork.ProcurementOrders.GetWithItemsAsync(procurementOrderId, cancellationToken);
        }

        private async Task EnsureInventoryStockExistsAsync(Guid inventoryId, Guid supplyItemId, CancellationToken cancellationToken)
        {
            var stock = await _unitOfWork.InventoryStocks.GetByInventoryAndSupplyItemAsync(inventoryId, supplyItemId, cancellationToken);
            if (stock != null)
            {
                return;
            }

            await _unitOfWork.InventoryStocks.AddAsync(new InventoryStock
            {
                InventoryStockId = Guid.NewGuid(),
                InventoryId = inventoryId,
                SupplyItemId = supplyItemId,
                CurrentQuantity = 0,
                MinimumStockLevel = 0,
                MaximumStockLevel = int.MaxValue
            });
        }

        private async Task<string> GenerateOrderCodeAsync(CancellationToken cancellationToken)
        {
            var today = DateTime.UtcNow.Date;
            var count = await _unitOfWork.ProcurementOrders.CountCreatedOnDateAsync(today, cancellationToken);
            return $"PO-{today:yyyyMMdd}-{(count + 1):D3}";
        }

        private static ProcurementOrderResponse Map(ProcurementOrder order)
        {
            return new ProcurementOrderResponse
            {
                ProcurementOrderId = order.ProcurementOrderId,
                CampaignId = order.CampaignId,
                DestinationInventoryId = order.DestinationInventoryId,
                OrderCode = order.OrderCode,
                Status = order.Status,
                TotalEstimatedCost = order.TotalEstimatedCost,
                TotalActualCost = order.TotalActualCost,
                SupplierName = order.SupplierName,
                SupplierContact = order.SupplierContact,
                Notes = order.Notes,
                ApprovalNote = order.ApprovalNote,
                ReceiveNote = order.ReceiveNote,
                CreatedBy = order.CreatedBy,
                CreatedAt = order.CreatedAt,
                ApprovedBy = order.ApprovedBy,
                ApprovedAt = order.ApprovedAt,
                ReceivedBy = order.ReceivedBy,
                ReceivedAt = order.ReceivedAt,
                InventoryTransactionId = order.InventoryTransactionId,
                Items = order.Items.Select(i => new ProcurementOrderItemResponse
                {
                    ProcurementOrderItemId = i.ProcurementOrderItemId,
                    SupplyItemId = i.SupplyItemId,
                    SupplyItemName = i.SupplyItem?.Name ?? string.Empty,
                    Quantity = i.Quantity,
                    UnitCost = i.UnitCost,
                    ReceivedQuantity = i.ReceivedQuantity,
                    ActualUnitCost = i.ActualUnitCost
                }).ToList()
            };
        }
    }
}
