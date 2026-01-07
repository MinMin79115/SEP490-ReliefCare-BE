using ReliefManagementSystem.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Application.Features.Team
{
    public class UpdateTeamRequest
    {
        public string Name { get; set; } = null!;

        public string? Description { get; set; }

        public TeamStatus Status { get; set; }

        // Moderator can change leader when update team
        public Guid? LeaderId { get; set; }
    }
}
