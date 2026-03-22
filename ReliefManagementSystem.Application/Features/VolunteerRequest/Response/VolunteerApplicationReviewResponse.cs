using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Application.Features.VolunteerRequest.Response
{
    public class VolunteerApplicationReviewResponse
    {
        public Guid VolunteerProfileId { get; set; }
        public Guid UserId { get; set; }

        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }

        public DateTime? AppliedAt { get; set; }
        public VerificationStatus VerificationStatus { get; set; }
        public VolunteerStatus Status { get; set; }
        public Guid? VerifiedBy { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public string? Reason { get; set; }

        public string? Descriptions { get; set; }
        public int? YearsOfExperience { get; set; }
        public TeamRolePreference PreferredTeamRole { get; set; }
        public VolunteerType VolunteerType { get; set; }

        public List<VolunteerSkillResponse> Skills { get; set; } = new();
        public List<VolunteerCertificateResponse> Certificates { get; set; } = new();
    }
}
