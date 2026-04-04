using System.ComponentModel.DataAnnotations;

namespace ReliefManagementSystem.Application.Features.ReliefRequest.DTOs.Request
{
    public class ApproveReliefRequestDto
    {
        [MaxLength(500)]
        public string? Note { get; set; }
    }
}
