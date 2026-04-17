using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Application.Features.VolunteerRequest.Request
{
    public class ResubmitVolunteerRequest
    {
        public Guid? CampaignId { get; set; }
        public string? Descriptions { get; set; }
        public int? YearsOfExperience { get; set; }
        public TeamRolePreference PreferredTeamRole { get; set; }
        public List<Guid> SkillIds { get; set; } = new();
        public List<CreateVolunteerCertificateRequest> Certificates { get; set; } = new();
    }
}
