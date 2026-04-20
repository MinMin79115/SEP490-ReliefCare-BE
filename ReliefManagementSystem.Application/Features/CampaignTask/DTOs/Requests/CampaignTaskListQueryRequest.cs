using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Application.Features.CampaignTask.DTOs.Requests
{
    public class CampaignTaskListQueryRequest
    {
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public CampaignTaskStatus? Status { get; set; }
        public Guid? CampaignTeamId { get; set; }
    }
}
