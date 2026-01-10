using ReliefManagementSystem.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Application.Features.Team.DTOs.Response
{
    public class TeamResponse
    {
        public Guid TeamId { get; set; }

        public string Name { get; set; } = null!;

        public string? Description { get; set; }

        public TeamStatus Status { get; set; }

        // Moderator control team
        public Guid ModeratorId { get; set; }
        public string ModeratorName { get; set; } = null!;

        // Leader  (can null)
        public Guid? LeaderId { get; set; }
        public string? LeaderName { get; set; }

        public int TotalMembers { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}
