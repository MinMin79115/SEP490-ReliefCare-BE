using Microsoft.AspNetCore.Identity;
using ReliefManagementSystem.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Domain.Entities
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        public string? PictureUrl { get; set; }

        public string? PicturePublicId { get; set; }

        public DateTime? DateOfBirth { get; set; }

        public string? Gender { get; set; }

        public string? DisplayName { get; set; }

        public VolunteerProfile VolunteerProfile { get; set; }
        public ICollection<TeamMember> TeamMembers { get; set; }

    }
}
