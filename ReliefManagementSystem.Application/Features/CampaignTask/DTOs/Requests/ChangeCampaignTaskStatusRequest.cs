using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Application.Features.CampaignTask.DTOs.Requests
{
    public class ChangeCampaignTaskStatusRequest
    {
        public CampaignTaskStatus Status { get; set; }
    }
}
