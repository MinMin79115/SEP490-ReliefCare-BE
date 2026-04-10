namespace ReliefManagementSystem.Application.Features.User
{
    public class UpdateManagerAccountRequest
    {
        public string? Email { get; set; }
        public string? UserName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? FullName { get; set; }
        public string? Notes { get; set; }
    }
}
