using ReliefManagementSystem.Application.Features.InventoryTransaction.DTOs.Request;
using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Application.Interface
{
    public interface ICampaignInventoryService
    {
        Task<CampaignInventory> EnsureCampaignInventoryAsync(Guid campaignId, CancellationToken cancellationToken = default);

        Task<CampaignInventoryTransaction> CreateTransactionAsync(
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
            CancellationToken cancellationToken = default);
    }
}
