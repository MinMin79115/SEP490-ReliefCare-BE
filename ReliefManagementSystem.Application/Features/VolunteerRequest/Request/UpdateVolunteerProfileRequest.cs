using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Application.Features.VolunteerRequest.Request
{
    public class UpdateVolunteerProfileRequest
    {
        public string? Descriptions { get; set; }
        public int? YearsOfExperience { get; set; }
        public TeamRolePreference PreferredTeamRole { get; set; } = TeamRolePreference.Member;
        public List<Guid> SkillIds { get; set; } = new();
        public List<CreateVolunteerCertificateRequest> Certificates { get; set; } = new();
    }
}
