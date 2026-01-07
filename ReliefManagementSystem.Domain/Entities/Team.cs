using ReliefManagementSystem.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Domain.Entities
{
    public class Team
    {
        public int TeamId { get; set; }

        public string Name { get; set; }

        public string? Description { get; set; }

        // Moderator control team
        public Guid ModeratorId { get; set; }
        public ApplicationUser Moderator { get; set; } = null!;

        // Team leader
        public Guid? LeaderId { get; set; }
        public ApplicationUser? Leader { get; set; }

        public TeamStatus Status { get; set; } = TeamStatus.Active;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public ICollection<TeamMember> TeamMembers { get; set; } = new List<TeamMember>();
        public ICollection<TeamJoinRequest> TeamJoinRequests { get; set; } = new List<TeamJoinRequest>();
    }
}
