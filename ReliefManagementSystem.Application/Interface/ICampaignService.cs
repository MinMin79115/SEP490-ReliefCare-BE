using ReliefManagementSystem.Application.Common.Models;
using ReliefManagementSystem.Application.Features.Campaign.Dtos.Requests;
using ReliefManagementSystem.Application.Features.Campaign.Dtos.Responses;
using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Application.Interface
{
    public interface ICampaignService
    {
        Task<CampaignResponse> CreateAsync(CreateCampaignRequest request, CancellationToken cancellationToken = default);
        Task<CampaignResponse> GetByIdAsync(Guid campaignId, CancellationToken cancellationToken = default);
        Task<Pagination<CampaignSummaryResponse>> GetPagedAsync(CampaignListQueryRequest request, CancellationToken cancellationToken = default);
        Task<CampaignResponse> UpdateAsync(Guid campaignId, UpdateCampaignRequest request, CancellationToken cancellationToken = default);
        Task<CampaignResponse> ChangeStatusAsync(Guid campaignId, ChangeCampaignStatusRequest request, CancellationToken cancellationToken = default);
        Task<CampaignResponse> AttachStationAsync(Guid campaignId, AttachCampaignStationRequest request, CancellationToken cancellationToken = default);
        Task<CampaignResponse> DetachStationAsync(Guid campaignId, Guid reliefStationId, CancellationToken cancellationToken = default);
        Task<CampaignTeamResponse> AssignTeamAsync(Guid campaignId, AssignCampaignTeamRequest request, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<CampaignTeamResponse>> GetTeamsAsync(Guid campaignId, CancellationToken cancellationToken = default);
        Task<CampaignTeamResponse> UpdateTeamStatusAsync(Guid campaignId, Guid teamId, UpdateCampaignTeamStatusRequest request, CancellationToken cancellationToken = default);
        Task RemoveTeamAsync(Guid campaignId, Guid teamId, CancellationToken cancellationToken = default);
        Task UpdateProgressAsync(Guid campaignId, CampaignResourceType resourceType, decimal amountDelta, CancellationToken cancellationToken = default);
    }
}
