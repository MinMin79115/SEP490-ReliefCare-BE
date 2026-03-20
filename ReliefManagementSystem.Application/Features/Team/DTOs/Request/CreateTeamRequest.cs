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

        public string? ContactPhone { get; set; }

        // LeaderId removed - teams are created without leaders
        // Leader is assigned later via UpdateTeam by Moderator
    }
}
