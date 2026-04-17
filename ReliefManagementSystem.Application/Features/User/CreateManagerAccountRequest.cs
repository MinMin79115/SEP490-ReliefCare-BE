namespace ReliefManagementSystem.Application.Features.User
{
    public class CreateManagerAccountRequest
    {
        public string Email { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string? Notes { get; set; }
    }
}
