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

        public Guid? VerifiedBy { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public Guid UserId { get; set; }
        public ApplicationUser User { get; set; }
        public string? Descriptions { get; set; }
        public string? Reason { get; set; }
        public ICollection<VolunteerSkill> VolunteerSkills { get; set; }

    }
}
