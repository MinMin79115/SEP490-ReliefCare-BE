using ReliefManagementSystem.Domain.Enum;
using System.ComponentModel.DataAnnotations;

namespace ReliefManagementSystem.Application.Features.PriorityCriteria.DTOs.Request
{
    public class UpdatePriorityCriteriaRequest
    {
        [Required(ErrorMessage = "Name is required.")]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "Point is required.")]
        public int Point { get; set; }

        [Required(ErrorMessage = "DisasterType is required.")]
        public DisasterType DisasterType { get; set; }

        [Required(ErrorMessage = "Code is required.")]
        public string Code { get; set; } = null!;

        public string? Description { get; set; }

        [Required(ErrorMessage = "Status is required.")]
        public string Status { get; set; } = null!;
    }
}
