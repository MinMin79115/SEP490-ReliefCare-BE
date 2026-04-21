using ReliefManagementSystem.Application.Common.Models;
using ReliefManagementSystem.Application.Features.CampaignTask.DTOs.Requests;
using ReliefManagementSystem.Application.Features.CampaignTask.DTOs.Responses;

namespace ReliefManagementSystem.Application.Interface
{
    public interface ICampaignTaskService
    {
        Task<CampaignTaskResponse> CreateAsync(Guid campaignId, CreateCampaignTaskRequest request, CancellationToken cancellationToken = default);
        Task<Pagination<CampaignTaskResponse>> GetPagedAsync(Guid campaignId, CampaignTaskListQueryRequest request, CancellationToken cancellationToken = default);
        Task<CampaignTaskDetailResponse> GetByIdAsync(Guid campaignTaskId, CancellationToken cancellationToken = default);
        Task<CampaignTaskResponse> UpdateAsync(Guid campaignTaskId, UpdateCampaignTaskRequest request, CancellationToken cancellationToken = default);
        Task<CampaignTaskResponse> ChangeStatusAsync(Guid campaignTaskId, ChangeCampaignTaskStatusRequest request, CancellationToken cancellationToken = default);
        Task<MemberTaskResponse> AssignMemberAsync(Guid campaignTaskId, AssignMemberTaskRequest request, CancellationToken cancellationToken = default);
        Task<List<MemberTaskResponse>> BulkAssignMembersAsync(Guid campaignTaskId, BulkAssignMembersTaskRequest request, CancellationToken cancellationToken = default);
        Task DeleteAsync(Guid campaignTaskId, CancellationToken cancellationToken = default);
    }
}
