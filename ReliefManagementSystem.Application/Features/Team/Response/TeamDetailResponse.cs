using ReliefManagementSystem.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Application.Features.Team.Response
{
    public class TeamDetailResponse
    {
        public int TeamId { get; set; }

        public string Name { get; set; } = null!;

        public string? Description { get; set; }

        public TeamStatus Status { get; set; }

        public ModeratorInfo Moderator { get; set; } = null!;

        public LeaderInfo? Leader { get; set; }

        public List<TeamMemberInfo> Members { get; set; } = new();

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }

    public class ModeratorInfo
    {
        public Guid UserId { get; set; }
        public string DisplayName { get; set; } = null!;
        public string Email { get; set; } = null!;
    }

    public class LeaderInfo
    {
        public Guid UserId { get; set; }
        public string DisplayName { get; set; } = null!;
        public string Email { get; set; } = null!;
        // Skills from VolunteerProfile
        public List<SkillInfo> Skills { get; set; } = new();
    }

    public class TeamMemberInfo
    {
        public Guid UserId { get; set; }
        public string DisplayName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public TeamRole Role { get; set; }
        // Skills từ VolunteerProfile
        public List<SkillInfo> Skills { get; set; } = new();
        public DateTime JoinedAt { get; set; }
    }

    public class SkillInfo
    {
        public int SkillId { get; set; }
        public string Code { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
    }
}
