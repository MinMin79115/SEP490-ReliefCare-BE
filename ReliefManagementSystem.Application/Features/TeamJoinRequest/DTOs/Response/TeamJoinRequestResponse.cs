using ReliefManagementSystem.Application.Features.Team.DTOs.Response;
using ReliefManagementSystem.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Application.Features.TeamJoinRequest.DTOs.Response
{
    public class TeamJoinRequestResponse
    {
        public Guid Id { get; set; }

        public Guid TeamId { get; set; }
        public string TeamName { get; set; } = null!;
        public Guid ModeratorId { get; set; }
        public string ModeratorName { get; set; } = null!;

        public Guid VolunteerId { get; set; }
        public string VolunteerName { get; set; } = null!;
        public string VolunteerEmail { get; set; } = null!;

        public List<SkillInfo> VolunteerSkills { get; set; } = new();

        public TeamJoinRequestStatus Status { get; set; }
        public string? Reason { get; set; }

        public Guid? ApprovedBy { get; set; }
        public DateTime? ApprovedAt { get; set; }
        
        public Guid? RejectedBy { get; set; }
        public DateTime? RejectedAt { get; set; }
        
        public string? ReviewNote { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
