using ReliefManagementSystem.Domain.Enum;
using System.ComponentModel.DataAnnotations;

namespace ReliefManagementSystem.Application.Features.Team.DTOs.Request
{
    public class UpdateTeamRequest
    {
        [Required(ErrorMessage = "Name is required")]
        [MaxLength(150)]
        public string Name { get; set; } = null!;

        [MaxLength(1000)]
        public string? Description { get; set; }

        [MaxLength(20)]
        public string? ContactPhone { get; set; }

        public TeamStatus Status { get; set; }

        // Moderator can change leader when update team
        public Guid? LeaderId { get; set; }
    }
}
