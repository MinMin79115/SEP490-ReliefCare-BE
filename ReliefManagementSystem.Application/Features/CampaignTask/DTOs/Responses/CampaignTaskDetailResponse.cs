using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Application.Features.CampaignTask.DTOs.Responses
{
    public class CampaignTaskDetailResponse : CampaignTaskResponse
    {
        public int MemberTaskCount { get; set; }
        public int CompletedMemberTaskCount { get; set; }
        public List<MemberTaskResponse> MemberTasks { get; set; } = [];
    }
}
