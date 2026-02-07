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
        public string? Reason { get; set; } = null!;
    }
}
