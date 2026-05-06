using ReliefManagementSystem.Application.Features.Relief.DTOs.Request;
using ReliefManagementSystem.Application.Features.Relief.DTOs.Response;
using ReliefManagementSystem.Application.Common.Models;
using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Application.Interface
{
    public interface IReliefDistributionService
    {
        Task<IReadOnlyList<CampaignHouseholdResponse>> ImportCampaignHouseholdsAsync(
            Guid campaignId,
            ImportCampaignHouseholdsRequest request,
            CancellationToken cancellationToken = default);

        Task<CampaignHouseholdResponse> ReportNewReliefHouseholdAsync(
            Guid campaignId,
            ReportNewReliefHouseholdRequest request,
            CancellationToken cancellationToken = default);

        Task<HouseholdDeliveryResponse> AssignHouseholdAsync(
            Guid campaignId,
            Guid campaignHouseholdId,
            AssignHouseholdRequest request,
            CancellationToken cancellationToken = default);

        Task<AssignIsolatedHouseholdTeamResponse> AssignIsolatedHouseholdTeamAsync(
            Guid campaignId,
            Guid campaignHouseholdId,
            AssignIsolatedHouseholdTeamRequest request,
            CancellationToken cancellationToken = default);

        Task<BulkAssignIsolatedHouseholdsResponse> BulkAssignIsolatedHouseholdTeamsAsync(
            Guid campaignId,
            BulkAssignIsolatedHouseholdsRequest request,
            CancellationToken cancellationToken = default);

        Task<Pagination<CampaignHouseholdResponse>> GetCampaignHouseholdsAsync(
            Guid campaignId,
            HouseholdQueryRequest request,
            CancellationToken cancellationToken = default);

        Task<ReliefCampaignPlanSummaryResponse> GetCampaignPlanSummaryAsync(
            Guid campaignId,
            CancellationToken cancellationToken = default);

        Task<Pagination<HouseholdChecklistItemResponse>> GetChecklistAsync(
            Guid campaignId,
            DeliveryQueryRequest request,
            CancellationToken cancellationToken = default);

        Task<Pagination<TeamDeliveryWorklistItemResponse>> GetTeamDeliveryWorklistAsync(
            Guid campaignId,
            TeamDeliveryWorklistQueryRequest request,
            CancellationToken cancellationToken = default);

        Task<CampaignHouseholdResponse> UpdateCampaignHouseholdAsync(
            Guid campaignId,
            Guid campaignHouseholdId,
            UpdateCampaignHouseholdRequest request,
            CancellationToken cancellationToken = default);

        Task<CampaignHouseholdResponse> UpdateCampaignHouseholdStatusAsync(
            Guid campaignId,
            Guid campaignHouseholdId,
            UpdateCampaignHouseholdStatusRequest request,
            CancellationToken cancellationToken = default);

        Task DeleteCampaignHouseholdAsync(
            Guid campaignId,
            Guid campaignHouseholdId,
            CancellationToken cancellationToken = default);

        Task<DistributionPointResponse> CreateDistributionPointAsync(
            Guid campaignId,
            CreateDistributionPointRequest request,
            CancellationToken cancellationToken = default);

        Task<Pagination<DistributionPointResponse>> GetDistributionPointsAsync(
            Guid campaignId,
            DistributionPointQueryRequest request,
            CancellationToken cancellationToken = default);

        Task<DistributionPointResponse> UpdateDistributionPointAsync(
            Guid campaignId,
            Guid distributionPointId,
            UpdateDistributionPointRequest request,
            CancellationToken cancellationToken = default);

        Task<DistributionPointResponse> DeleteDistributionPointAsync(
            Guid campaignId,
            Guid distributionPointId,
            CancellationToken cancellationToken = default);

        Task<ReliefPackageDefinitionResponse> CreateReliefPackageDefinitionAsync(
            Guid campaignId,
            CreateReliefPackageDefinitionRequest request,
            CancellationToken cancellationToken = default);

        Task<Pagination<ReliefPackageDefinitionResponse>> GetReliefPackageDefinitionsAsync(
            Guid campaignId,
            ReliefPackageQueryRequest request,
            CancellationToken cancellationToken = default);

        Task<ReliefPackageDefinitionResponse> UpdateReliefPackageDefinitionAsync(
            Guid campaignId,
            Guid reliefPackageDefinitionId,
            UpdateReliefPackageDefinitionRequest request,
            CancellationToken cancellationToken = default);

        Task<ReliefPackageDefinitionResponse> DeleteReliefPackageDefinitionAsync(
            Guid campaignId,
            Guid reliefPackageDefinitionId,
            CancellationToken cancellationToken = default);

        Task<ReliefPackageAssemblyAvailabilityResponse> GetPackageAssemblyAvailabilityAsync(
            Guid campaignId,
            Guid reliefPackageDefinitionId,
            Guid reliefStationId,
            Guid inventoryId,
            CancellationToken cancellationToken = default);

        Task<ReliefPackageAssemblyResponse> AssembleReliefPackageAsync(
            Guid campaignId,
            Guid reliefPackageDefinitionId,
            AssembleReliefPackageRequest request,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<ReliefPackageAssemblyResponse>> GetPackageAssemblyHistoryByCampaignAsync(
            Guid campaignId,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<ReliefPackageAssemblyResponse>> GetPackageAssemblyHistoryByStationAsync(
            Guid campaignId,
            Guid reliefStationId,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<ReliefPackageAssemblyResponse>> GetPackageAssemblyHistoryByDefinitionAsync(
            Guid campaignId,
            Guid reliefPackageDefinitionId,
            CancellationToken cancellationToken = default);

        Task<HouseholdDeliveryResponse> CompleteHouseholdDeliveryAsync(
            Guid campaignId,
            Guid householdDeliveryId,
            CompleteHouseholdDeliveryRequest request,
            CancellationToken cancellationToken = default);

        Task<Pagination<HouseholdDeliveryResponse>> GetDeliveriesAsync(
            Guid campaignId,
            DeliveryQueryRequest request,
            CancellationToken cancellationToken = default);

        Task<HouseholdDeliveryResponse> GetDeliveryByIdAsync(
            Guid campaignId,
            Guid householdDeliveryId,
            CancellationToken cancellationToken = default);

        Task<HouseholdDeliveryResponse> UpdateHouseholdDeliveryAssignmentAsync(
            Guid campaignId,
            Guid householdDeliveryId,
            UpdateHouseholdDeliveryAssignmentRequest request,
            CancellationToken cancellationToken = default);

        Task DeleteHouseholdDeliveryAssignmentAsync(
            Guid campaignId,
            Guid householdDeliveryId,
            CancellationToken cancellationToken = default);

        Task<BatchCompleteHouseholdDeliveryResponse> CompleteHouseholdDeliveriesBatchAsync(
            Guid campaignId,
            CompleteHouseholdDeliveryBatchRequest request,
            CancellationToken cancellationToken = default);

        Task<SupplyShortageRequestResponse> CreateShortageRequestAsync(
            Guid campaignId,
            CreateSupplyShortageRequest request,
            CancellationToken cancellationToken = default);

        Task<Pagination<SupplyShortageRequestResponse>> GetShortageRequestsAsync(
            Guid campaignId,
            SupplyShortageRequestQueryRequest request,
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
