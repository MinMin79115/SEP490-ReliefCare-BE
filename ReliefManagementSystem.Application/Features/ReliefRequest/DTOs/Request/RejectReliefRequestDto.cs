using System.ComponentModel.DataAnnotations;

namespace ReliefManagementSystem.Application.Features.ReliefRequest.DTOs.Request
{
    public class RejectReliefRequestDto
    {
        [MaxLength(500)]
        public string? Reason { get; set; }
    }
}
