namespace ReliefManagementSystem.Application.Features.Auth.DTOs
{
    public class VerifyEmailOtpRequest
    {
        public string Email { get; set; } = null!;
        public string Code { get; set; } = null!;
    }
}
