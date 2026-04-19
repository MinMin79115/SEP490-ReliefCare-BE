using ReliefManagementSystem.Application.Common.Interface;
using ReliefManagementSystem.Application.Features.InventoryTransaction.DTOs.Request;
using ReliefManagementSystem.Application.Interface;
using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Application.Services
{
    public class CampaignInventoryService : ICampaignInventoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;

        public CampaignInventoryService(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
        {
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
        }

        public async Task<CampaignInventory> EnsureCampaignInventoryAsync(Guid campaignId, CancellationToken cancellationToken = default)
        {
            var campaign = await _unitOfWork.Campaigns.GetWithDetailsAsync(campaignId, cancellationToken)
                ?? throw new KeyNotFoundException($"Campaign '{campaignId}' was not found.");

            var existing = await _unitOfWork.CampaignInventories.GetByCampaignIdWithDetailsAsync(campaignId, cancellationToken);
            if (existing is not null)
            {
                return existing;
            }

            var created = new CampaignInventory
            {
                CampaignInventoryId = Guid.NewGuid(),
                CampaignId = campaign.CampaignId,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.CampaignInventories.AddAsync(created);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return await _unitOfWork.CampaignInventories.GetByCampaignIdWithDetailsAsync(campaignId, cancellationToken)
                ?? created;
        }

        public async Task<CampaignInventoryTransaction> CreateTransactionAsync(
            Guid campaignId,
            TransactionType type,
            TransactionReason reason,
            IReadOnlyCollection<TransactionItemRequest> items,
            string? notes = null,
            Guid? supplyAllocationId = null,
            Guid? campaignTeamId = null,
            Guid? distributionPointId = null,
            Guid? householdDeliveryId = null,
            Guid? reliefPackageDefinitionId = null,
            bool autoSave = true,
            CancellationToken cancellationToken = default)
        {
            if (items is null || items.Count == 0)
                throw new InvalidOperationException("At least one campaign inventory item is required.");

            var duplicates = items
                .GroupBy(i => i.SupplyItemId)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (duplicates.Count != 0)
                throw new InvalidOperationException($"Duplicate campaign inventory items in request: {string.Join(", ", duplicates)}");

            var campaignInventory = await EnsureCampaignInventoryAsync(campaignId, cancellationToken);
            var stocks = await _unitOfWork.CampaignInventoryStocks.GetByCampaignInventoryIdForUpdateAsync(campaignInventory.CampaignInventoryId, cancellationToken);
            var updates = new List<(CampaignInventoryStock stock, int newQty)>();
            var newStocks = new List<CampaignInventoryStock>();

            foreach (var item in items)
            {
                var stock = stocks.FirstOrDefault(x => x.SupplyItemId == item.SupplyItemId);

                if (type == TransactionType.Export)
                {
                    if (stock is null || stock.CurrentQuantity < item.Quantity)
                    {
                        throw new InvalidOperationException(
                            $"Insufficient campaign stock for supply item '{item.SupplyItemId}'.");
                    }

                    updates.Add((stock, stock.CurrentQuantity - item.Quantity));
                    continue;
                }

                if (stock is null)
                {
                    stock = new CampaignInventoryStock
                    {
                        CampaignInventoryStockId = Guid.NewGuid(),
                        CampaignInventoryId = campaignInventory.CampaignInventoryId,
                        SupplyItemId = item.SupplyItemId,
                        CurrentQuantity = 0
                    };

                    newStocks.Add(stock);
                    stocks.Add(stock);
                }

                updates.Add((stock, stock.CurrentQuantity + item.Quantity));
            }

            foreach (var stock in newStocks)
            {
                await _unitOfWork.CampaignInventoryStocks.AddAsync(stock);
            }

            var transaction = new CampaignInventoryTransaction
            {
                CampaignInventoryTransactionId = Guid.NewGuid(),
                CampaignInventoryId = campaignInventory.CampaignInventoryId,
                TransactionCode = await GenerateTransactionCodeAsync(type, cancellationToken),
                Type = type,
                Reason = reason,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = _currentUser.UserId ?? throw new UnauthorizedAccessException("User is not authenticated."),
                Notes = notes,
                SupplyAllocationId = supplyAllocationId,
                CampaignTeamId = campaignTeamId,
                DistributionPointId = distributionPointId,
                HouseholdDeliveryId = householdDeliveryId,
                ReliefPackageDefinitionId = reliefPackageDefinitionId,
                Items = items.Select(i => new CampaignInventoryTransactionItem
                {
                    CampaignInventoryTransactionItemId = Guid.NewGuid(),
                    SupplyItemId = i.SupplyItemId,
                    Quantity = i.Quantity,
                    Notes = i.Notes
                }).ToList()
            };

            await _unitOfWork.CampaignInventoryTransactions.AddAsync(transaction);

            foreach (var (stock, newQty) in updates)
            {
                stock.CurrentQuantity = newQty;
                await _unitOfWork.CampaignInventoryStocks.UpdateAsync(stock);
            }

            if (autoSave)
            {
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            return transaction;
        }

        private async Task<string> GenerateTransactionCodeAsync(TransactionType type, CancellationToken cancellationToken)
        {
            var prefix = type == TransactionType.Import ? "CIN" : "COUT";
            var date = DateTime.UtcNow.ToString("yyyyMMdd");
            var count = await _unitOfWork.CampaignInventoryTransactions.CountTodayByTypeAsync(type, cancellationToken);
            return $"{prefix}-{date}-{(count + 1):D3}";
        }
    }
}
