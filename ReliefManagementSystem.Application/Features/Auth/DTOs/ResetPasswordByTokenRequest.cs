using System.ComponentModel.DataAnnotations;

namespace ReliefManagementSystem.Application.Features.Auth.DTOs
{
    public class ResetPasswordByTokenRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        public string ResetToken { get; set; } = null!;

        [Required]
        [MinLength(6)]
        public string NewPassword { get; set; } = null!;
    }
}
