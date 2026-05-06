using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Application.Features.CampaignTask.DTOs.Responses
{
    public class CampaignTaskDetailResponse : CampaignTaskResponse
    {
        public int MemberTaskCount { get; set; }
        public int CompletedMemberTaskCount { get; set; }
        public List<MemberTaskResponse> MemberTasks { get; set; } = [];
    }

    public class AdminCampaignTaskAggregateResponse : CampaignTaskDetailResponse
    {
        public Guid TeamId { get; set; }
        public string TeamName { get; set; } = string.Empty;
        public string TeamType { get; set; } = string.Empty;
        public int TeamMemberCount { get; set; }
        public string CampaignName { get; set; } = string.Empty;
        public string CampaignStatus { get; set; } = string.Empty;
    }

    public class AdminTopTeamResponse
    {
        public Guid TeamId { get; set; }
        public string TeamName { get; set; } = string.Empty;
        public string TeamType { get; set; } = string.Empty;
        public Guid? CampaignId { get; set; }
        public string CampaignName { get; set; } = string.Empty;
        public int TeamMemberCount { get; set; }
        public int TaskCount { get; set; }
        public int MemberTaskCount { get; set; }
        public int CompletedMemberTaskCount { get; set; }
        public int InProgressMemberTaskCount { get; set; }
        public int FailedMemberTaskCount { get; set; }
        public string? TopVolunteerName { get; set; }
        public int TopVolunteerScore { get; set; }
        public DateTime? LatestTaskDate { get; set; }
        public int AssignedRescueRequestCount { get; set; }
        public int CompletedRescueRequestCount { get; set; }
        public decimal ImpactScore { get; set; }
    }
}
