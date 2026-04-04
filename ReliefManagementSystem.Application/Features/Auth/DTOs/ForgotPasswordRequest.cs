using System.ComponentModel.DataAnnotations;

namespace ReliefManagementSystem.Application.Features.Auth.DTOs
{
    public class ForgotPasswordRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
