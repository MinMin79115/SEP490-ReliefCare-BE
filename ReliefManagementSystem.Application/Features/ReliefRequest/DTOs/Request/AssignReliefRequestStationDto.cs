using System.ComponentModel.DataAnnotations;

namespace ReliefManagementSystem.Application.Features.ReliefRequest.DTOs.Request
{
    public class AssignReliefRequestStationDto
    {
        [Required]
        public Guid ReliefStationId { get; set; }
    }
}
