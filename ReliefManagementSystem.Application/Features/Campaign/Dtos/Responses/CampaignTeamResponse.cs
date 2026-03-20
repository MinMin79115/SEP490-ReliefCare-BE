using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Application.Features.Campaign.Dtos.Responses
{
    public class CampaignTeamResponse
    {
        public Guid CampaignTeamId { get; set; }
        public Guid CampaignId { get; set; }
        public Guid TeamId { get; set; }
        public string TeamName { get; set; } = string.Empty;
        public CampaignTeamRole Role { get; set; }
        public CampaignTeamStatus Status { get; set; }
        public DateTime AssignedAt { get; set; }
        public int MemberCount { get; set; }
    }
}
