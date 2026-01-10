using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Application.Features.TeamJoinRequest.DTOs.Request
{
    public class ReviewTeamJoinRequest
    {
        public bool IsApproved { get; set; }
        public string? ReviewNote { get; set; }

    }
}
