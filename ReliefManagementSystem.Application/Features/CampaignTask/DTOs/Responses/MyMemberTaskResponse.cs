using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Application.Features.CampaignTask.DTOs.Responses
{
    public class MyMemberTaskResponse
    {
        public Guid MemberTaskId { get; set; }
        public Guid CampaignTaskId { get; set; }
        public Guid CampaignId { get; set; }
        public Guid CampaignTeamId { get; set; }
        public string CampaignTeamName { get; set; } = string.Empty;
        public string CampaignTaskTitle { get; set; } = string.Empty;
        public string? CampaignTaskDescription { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? DueDate { get; set; }
        public CampaignTaskStatus CampaignTaskStatus { get; set; }
        public TaskPriority Priority { get; set; }
        public Guid VolunteerProfileId { get; set; }
        public string VolunteerName { get; set; } = string.Empty;
        public string SubTaskTitle { get; set; } = string.Empty;
        public string? TaskNote { get; set; }
        public string? FailureReason { get; set; }
        public DateTime? AssignedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public MemberTaskStatus Status { get; set; }
        public List<MemberTaskDeliveryResponse> Deliveries { get; set; } = [];
    }
}
