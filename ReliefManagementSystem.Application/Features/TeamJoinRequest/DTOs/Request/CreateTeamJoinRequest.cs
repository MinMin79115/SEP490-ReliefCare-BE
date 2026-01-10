using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Application.Features.TeamJoinRequest.DTOs.Request
{
    public class CreateTeamJoinRequest
    {
        public Guid TeamId { get; set; }
        // RequestedRole removed - volunteers can only join as Member
        // Leader role is assigned by Moderator via UpdateTeam
    }
}
