using System.ComponentModel.DataAnnotations;

namespace ReliefManagementSystem.Application.Features.ReliefRequest.DTOs.Request
{
    public class CompleteReliefRequestDto
    {
        [MaxLength(1000)]
        public string? Note { get; set; }
    }
}
