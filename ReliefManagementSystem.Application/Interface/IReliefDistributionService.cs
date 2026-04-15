using Microsoft.AspNetCore.Http;
using ReliefManagementSystem.Application.Features.Relief.DTOs.Request;
using ReliefManagementSystem.Application.Features.Relief.DTOs.Response;
using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Application.Interface
{
    public interface IReliefDistributionService
    {
        Task<IReadOnlyList<CampaignHouseholdResponse>> ImportCampaignHouseholdsAsync(
            Guid campaignId,
            ImportCampaignHouseholdsRequest request,
            CancellationToken cancellationToken = default);

        Task<HouseholdDeliveryResponse> AssignHouseholdAsync(
            Guid campaignId,
            Guid campaignHouseholdId,
            AssignHouseholdRequest request,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<CampaignHouseholdResponse>> GetCampaignHouseholdsAsync(
            Guid campaignId,
            HouseholdFulfillmentStatus? status,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<HouseholdChecklistItemResponse>> GetChecklistAsync(
            Guid campaignId,
            Guid? campaignTeamId,
            HouseholdFulfillmentStatus? status,
            CancellationToken cancellationToken = default);

        Task<DistributionPointResponse> CreateDistributionPointAsync(
            Guid campaignId,
            CreateDistributionPointRequest request,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<DistributionPointResponse>> GetDistributionPointsAsync(
            Guid campaignId,
            CancellationToken cancellationToken = default);

        Task<ReliefPackageDefinitionResponse> CreateReliefPackageDefinitionAsync(
            Guid campaignId,
            CreateReliefPackageDefinitionRequest request,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<ReliefPackageDefinitionResponse>> GetReliefPackageDefinitionsAsync(
            Guid campaignId,
            CancellationToken cancellationToken = default);

        Task<HouseholdDeliveryResponse> CompleteHouseholdDeliveryAsync(
            Guid campaignId,
            Guid householdDeliveryId,
            CompleteHouseholdDeliveryRequest request,
            IFormFile proofImage,
            CancellationToken cancellationToken = default);

        Task<SupplyShortageRequestResponse> CreateShortageRequestAsync(
            Guid campaignId,
            CreateSupplyShortageRequest request,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<SupplyShortageRequestResponse>> GetShortageRequestsAsync(
            Guid campaignId,
            SupplyShortageRequestStatus? status,
            CancellationToken cancellationToken = default);

        Task<SupplyShortageRequestResponse> ApproveShortageRequestAsync(
            Guid campaignId,
            Guid shortageRequestId,
            ReviewSupplyShortageRequest request,
            CancellationToken cancellationToken = default);

        Task<SupplyShortageRequestResponse> RejectShortageRequestAsync(
            Guid campaignId,
            Guid shortageRequestId,
            ReviewSupplyShortageRequest request,
            CancellationToken cancellationToken = default);
    }
}
