using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Application.Features.CampaignTask.DTOs.Requests
{
    public class ChangeMemberTaskStatusRequest
    {
        public MemberTaskStatus Status { get; set; }
        public string? FailureReason { get; set; }
    }
}
