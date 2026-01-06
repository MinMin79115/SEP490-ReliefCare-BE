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

        public string Skills { get; set; }
        public string AvailabilityStatus { get; set; }
        public string VerificationStatus { get; set; } = "PENDING";

        public Guid? VerifiedBy { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public bool? IsLeader { get; set; }
        public ApplicationUser User { get; set; }
    }
}
