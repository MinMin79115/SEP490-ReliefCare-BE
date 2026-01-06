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
        public Guid UserId { get; set; }
        public VerificationStatus VerificationStatus { get; set; } = VerificationStatus.Pending;

        public Guid? VerifiedBy { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public ApplicationUser User { get; set; }
        public ICollection<VolunteerSkill> VolunteerSkills { get; set; }

    }
}
