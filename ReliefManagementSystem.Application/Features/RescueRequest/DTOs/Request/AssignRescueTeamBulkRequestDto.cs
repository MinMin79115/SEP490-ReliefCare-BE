using System.ComponentModel.DataAnnotations;

namespace ReliefManagementSystem.Application.Features.RescueRequest.DTOs.Request
{
    public class AssignRescueTeamBulkRequestDto
    {
        [Required]
        public Guid TeamId { get; set; }

        [Required]
        [MinLength(1)]
        public List<Guid> RequestIds { get; set; } = new();

        public string? Note { get; set; }
    }
}
