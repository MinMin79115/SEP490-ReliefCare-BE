using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Application.Features.CampaignTask.DTOs.Requests
{
    public class MyMemberTaskQueryRequest
    {
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public MemberTaskStatus? Status { get; set; }
        public Guid? CampaignTeamId { get; set; }
    }
}
