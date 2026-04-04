using System.ComponentModel.DataAnnotations;

namespace ReliefManagementSystem.Application.Features.Auth.DTOs
{
    public class VerifyForgotPasswordOtpRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        [StringLength(6, MinimumLength = 6)]
        public string OtpCode { get; set; } = null!;
    }
}
