using System.ComponentModel.DataAnnotations;

namespace ReliefManagementSystem.Application.Features.Team.DTOs.Request
{
    public class CreateTeamRequest
    {
        [Required(ErrorMessage = "Name is required")]
        [MaxLength(150)]
        public string Name { get; set; } = null!;

        [MaxLength(1000)]
        public string? Description { get; set; }

        [MaxLength(20)]
        public string? ContactPhone { get; set; }

        // LeaderId removed - teams are created without leaders
        // Leader is assigned later via UpdateTeam by Moderator
    }
}
