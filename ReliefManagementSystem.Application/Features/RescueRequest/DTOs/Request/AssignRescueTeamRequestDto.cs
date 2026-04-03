using System.ComponentModel.DataAnnotations;

namespace ReliefManagementSystem.Application.Features.RescueRequest.DTOs.Request
{
    public class AssignRescueTeamRequestDto
    {
        [Required]
        public Guid TeamId { get; set; }

        public string? Note { get; set; }
    }
}
