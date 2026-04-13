namespace ReliefManagementSystem.Application.Features.User
{
    public class ManagerProfileResponse
    {
        public Guid Id { get; set; }
        public string? DisplayName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? PictureUrl { get; set; }
        public bool IsBanned { get; set; }
        public DateTimeOffset? LockoutEnd { get; set; }
        public string? BanReason { get; set; }
        public DateTime AppointedAt { get; set; }
        public string? Notes { get; set; }
    }
}
