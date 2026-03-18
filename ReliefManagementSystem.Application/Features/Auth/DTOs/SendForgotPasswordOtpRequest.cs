using System.ComponentModel.DataAnnotations;

namespace ReliefManagementSystem.Application.Features.Auth.DTOs
{
    public class SendForgotPasswordOtpRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;
    }
}
