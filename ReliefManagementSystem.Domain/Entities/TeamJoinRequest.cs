using ReliefManagementSystem.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Domain.Entities
{
    public class TeamJoinRequest
    {
        public Guid Id { get; set; }

        public Guid TeamId { get; set; }
        public Team Team { get; set; } = null!;

        public Guid VolunteerId { get; set; }
        public ApplicationUser Volunteer { get; set; } = null!;

        public TeamRole RequestedRole { get; set; }

        public TeamJoinRequestStatus Status { get; set; } = TeamJoinRequestStatus.Pending;

        // Moderator review
        public Guid? ReviewedBy { get; set; }
        public ApplicationUser? Reviewer { get; set; }

        public string? ReviewNote { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ReviewedAt { get; set; }
    }
}
