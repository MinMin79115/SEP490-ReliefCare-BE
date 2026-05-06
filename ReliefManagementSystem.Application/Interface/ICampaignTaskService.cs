using ReliefManagementSystem.Application.Common.Models;
using ReliefManagementSystem.Application.Features.CampaignTask.DTOs.Requests;
using ReliefManagementSystem.Application.Features.CampaignTask.DTOs.Responses;

namespace ReliefManagementSystem.Application.Interface
{
    public interface ICampaignTaskService
    {
        Task<CampaignTaskResponse> CreateAsync(Guid campaignId, CreateCampaignTaskRequest request, CancellationToken cancellationToken = default);
        Task<Pagination<CampaignTaskResponse>> GetPagedAsync(Guid campaignId, CampaignTaskListQueryRequest request, CancellationToken cancellationToken = default);
        Task<List<AdminCampaignTaskAggregateResponse>> GetAdminTaskAggregateAsync(DateTime? from = null, DateTime? to = null, Guid? teamId = null, Guid? campaignId = null, CancellationToken cancellationToken = default);
        Task<List<AdminTopTeamResponse>> GetAdminTopTeamsAsync(DateTime? from = null, DateTime? to = null, Guid? teamId = null, Guid? campaignId = null, int top = 4, CancellationToken cancellationToken = default);
        Task<Pagination<MyMemberTaskResponse>> GetMyMemberTasksAsync(Guid campaignId, MyMemberTaskQueryRequest request, CancellationToken cancellationToken = default);
        Task<List<MemberTaskDeliveryResponse>> GetMyMemberTaskDeliveriesAsync(Guid campaignId, CancellationToken cancellationToken = default);
        Task<CampaignTaskDetailResponse> GetByIdAsync(Guid campaignTaskId, CancellationToken cancellationToken = default);
        Task<CampaignTaskResponse> UpdateAsync(Guid campaignTaskId, UpdateCampaignTaskRequest request, CancellationToken cancellationToken = default);
        Task<CampaignTaskResponse> ChangeStatusAsync(Guid campaignTaskId, ChangeCampaignTaskStatusRequest request, CancellationToken cancellationToken = default);
        Task<MemberTaskResponse> AssignMemberAsync(Guid campaignTaskId, AssignMemberTaskRequest request, CancellationToken cancellationToken = default);
        Task<List<MemberTaskResponse>> BulkAssignMembersAsync(Guid campaignTaskId, BulkAssignMembersTaskRequest request, CancellationToken cancellationToken = default);
        Task<List<MemberTaskResponse>> CreateMemberTasksFromHouseholdsAsync(Guid campaignTaskId, CreateMemberTaskFromHouseholdsRequest request, CancellationToken cancellationToken = default);
        Task<List<MemberTaskResponse>> BulkAssignDeliveriesToMembersAsync(Guid campaignTaskId, BulkAssignDeliveriesToMembersRequest request, CancellationToken cancellationToken = default);
        Task<List<MemberTaskDeliveryResponse>> AssignDeliveriesToMemberTaskAsync(Guid memberTaskId, AssignMemberTaskDeliveriesRequest request, CancellationToken cancellationToken = default);
        Task<List<MemberTaskDeliveryResponse>> GetMemberTaskDeliveriesAsync(Guid memberTaskId, CancellationToken cancellationToken = default);
        Task<MemberTaskDeliveryResponse> ChangeMemberTaskDeliveryStatusAsync(Guid memberTaskDeliveryId, ChangeMemberTaskDeliveryStatusRequest request, CancellationToken cancellationToken = default);
        Task<MemberTaskDeliveryResponse> CompleteMemberTaskDeliveryWithDeliveryAsync(Guid memberTaskDeliveryId, CompleteMemberTaskDeliveryWithDeliveryRequest request, CancellationToken cancellationToken = default);
        Task DeleteAsync(Guid campaignTaskId, CancellationToken cancellationToken = default);
        Task<MemberTaskResponse> ChangeMemberTaskStatusAsync(Guid memberTaskId, ChangeMemberTaskStatusRequest request, CancellationToken cancellationToken = default);
    }
}
