using ReliefManagementSystem.Domain.Enum;
using System;

namespace ReliefManagementSystem.Application.Features.Team.DTOs.Response
{
    public class TeamMemberResponse
    {
        public Guid UserId { get; set; }
        public string DisplayName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public TeamRole RoleTeam { get; set; }
        public DateTime JoinedAt { get; set; }
        public List<SkillInfo>? Skills { get; set; }
    }
}