using ReliefManagementSystem.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Domain.Entities
{
    public class TeamMember
    {
        public int TeamId { get; set; }
        public Team Team { get; set; }

        public Guid UserId { get; set; }
        public ApplicationUser User { get; set; }

        public TeamRole RoleTeam { get; set; }

        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    }
}
