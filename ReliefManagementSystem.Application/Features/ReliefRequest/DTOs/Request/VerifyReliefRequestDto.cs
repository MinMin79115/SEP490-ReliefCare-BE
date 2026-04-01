using ReliefManagementSystem.Domain.Enum;
using System.ComponentModel.DataAnnotations;

namespace ReliefManagementSystem.Application.Features.ReliefRequest.DTOs.Request
{
    public class VerifyReliefRequestDto
    {
        [Required]
        public RequestVerificationStatus Status { get; set; }

        [Required]
        public VerificationMethod Method { get; set; }

        [MaxLength(500)]
        public string? Note { get; set; }

        [MaxLength(500)]
        public string? Reason { get; set; }
    }
}
