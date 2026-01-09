using ReliefManagementSystem.Application.Features.Team.Response;
using ReliefManagementSystem.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Application.Features.TeamJoinRequest.Response
{
    public class TeamJoinRequestResponse
    {
        public Guid Id { get; set; }

        // Team info
        public Guid TeamId { get; set; }
        public string TeamName { get; set; } = null!;
        public Guid ModeratorId { get; set; }
        public string ModeratorName { get; set; } = null!;

        // Volunteer info
        public Guid VolunteerId { get; set; }
        public string VolunteerName { get; set; } = null!;
        public string VolunteerEmail { get; set; } = null!;

        // Skills of volunteer (from VolunteerProfile)
        public List<SkillInfo> VolunteerSkills { get; set; } = new();

        // Request info
        public TeamRole RequestedRole { get; set; }
        public TeamJoinRequestStatus Status { get; set; }

        // Review info
        public Guid? ReviewedBy { get; set; }
        public string? ReviewerName { get; set; }
        public string? ReviewNote { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? ReviewedAt { get; set; }
    }
}
