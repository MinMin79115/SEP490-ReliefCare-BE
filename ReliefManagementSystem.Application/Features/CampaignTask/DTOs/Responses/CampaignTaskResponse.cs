using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Application.Features.CampaignTask.DTOs.Responses
{
    public class CampaignTaskResponse
    {
        public Guid CampaignTaskId { get; set; }
        public Guid CampaignId { get; set; }
        public Guid CampaignTeamId { get; set; }
        public string CampaignTeamName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? DueDate { get; set; }
        public CampaignTaskStatus Status { get; set; }
        public TaskPriority Priority { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
