using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Domain.Entities
{
    public class CampaignVolunteerRegistration
    {
        public Guid CampaignVolunteerRegistrationId { get; set; }
        public Guid CampaignId { get; set; }
        public Guid UserId { get; set; }
        public CampaignVolunteerRegistrationStatus Status { get; set; } = CampaignVolunteerRegistrationStatus.Registered;
        public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
        public DateTime? CancelledAt { get; set; }

        public Campaign Campaign { get; set; } = default!;
        public ApplicationUser User { get; set; } = default!;
    }
}
