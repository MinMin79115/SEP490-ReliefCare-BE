using System.ComponentModel.DataAnnotations;

namespace ReliefManagementSystem.Application.Features.RescueRequest.DTOs.Request
{
    public class ReorderRescueBatchRequestDto
    {
        [Required]
        [MinLength(1)]
        public List<Guid> RequestIdsInOrder { get; set; } = new();
    }
}
