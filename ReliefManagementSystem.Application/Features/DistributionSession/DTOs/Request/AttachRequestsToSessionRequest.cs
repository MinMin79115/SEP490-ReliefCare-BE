using System.ComponentModel.DataAnnotations;

namespace ReliefManagementSystem.Application.Features.DistributionSession.DTOs.Request
{
    public class AttachRequestsToSessionRequest
    {
        [Required]
        [MinLength(1)]
        public List<DistributionSessionRequestInputDto> Requests { get; set; } = new();
    }

    public class DistributionSessionRequestInputDto
    {
        [Required]
        public Guid ReliefRequestId { get; set; }

        [MaxLength(500)]
        public string? PlannedNote { get; set; }
    }
}
