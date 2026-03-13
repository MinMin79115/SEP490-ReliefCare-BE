using ReliefManagementSystem.Domain.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Domain.Entities
{
    public class VolunteerProfile
    {
        [Key]
        public Guid VolunteerProfileId { get; set; }
        public VerificationStatus VerificationStatus { get; set; } = VerificationStatus.Pending;
        public VolunteerStatus Status { get; set; }
        public Guid? VerifiedBy { get; set; }
        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? VerifiedAt { get; set; }
        public Guid UserId { get; set; }
        public ApplicationUser User { get; set; }
        public string? Descriptions { get; set; }
        public string? Reason { get; set; }
        public int? YearsOfExperience { get; set; }

        public ICollection<VolunteerSkill> VolunteerSkills { get; set; } = new List<VolunteerSkill>();
        public ICollection<VolunteerCertificate> Certificates { get; set; } = new List<VolunteerCertificate>();
    }
}

