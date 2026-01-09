using ReliefManagementSystem.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Application.Features.TeamJoinRequest.Request
{
    public class CreateTeamJoinRequest
    {
        public Guid TeamId { get; set; }

        public TeamRole RequestedRole { get; set; }
    }
}
