using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Application.Features.Campaign.Dtos.Responses
{
    public class CampaignVolunteerRegistrationResponse
    {
        public Guid CampaignVolunteerRegistrationId { get; set; }
        public Guid CampaignId { get; set; }
        public Guid UserId { get; set; }
        public string UserDisplayName { get; set; } = string.Empty;
        public string? UserEmail { get; set; }
        public CampaignVolunteerRegistrationStatus Status { get; set; }
        public DateTime RegisteredAt { get; set; }
        public DateTime? CancelledAt { get; set; }
    }
}
