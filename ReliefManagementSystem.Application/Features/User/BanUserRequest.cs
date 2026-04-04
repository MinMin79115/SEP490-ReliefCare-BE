using System.ComponentModel.DataAnnotations;

namespace ReliefManagementSystem.Application.Features.User
{
    public class BanUserRequest
    {
        [Required]
        [MaxLength(500)]
        public string Reason { get; set; } = null!;
    }
}
