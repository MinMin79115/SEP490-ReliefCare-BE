using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Application.Features.Team.DTOs.Request
{
    public class CreateTeamRequest
    {
        public string Name { get; set; } = null!;

        public string? Description { get; set; }

        // Moderator can set null for leader when create team
        public Guid? LeaderId { get; set; }
    }
}
