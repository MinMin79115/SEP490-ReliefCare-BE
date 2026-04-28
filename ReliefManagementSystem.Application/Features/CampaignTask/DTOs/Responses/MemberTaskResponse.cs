using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Application.Features.CampaignTask.DTOs.Responses
{
    public class MemberTaskResponse
    {
        public Guid MemberTaskId { get; set; }
        public Guid CampaignTaskId { get; set; }
        public Guid VolunteerProfileId { get; set; }
        public string VolunteerName { get; set; } = string.Empty;
        public string SubTaskTitle { get; set; } = string.Empty;
        public string? TaskNote { get; set; }
        public DateTime? AssignedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public MemberTaskStatus Status { get; set; }
        public List<MemberTaskDeliveryResponse> Deliveries { get; set; } = [];
    }
}
