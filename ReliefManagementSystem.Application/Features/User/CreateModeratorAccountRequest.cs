using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Application.Features.User
{
    public class CreateModeratorAccountRequest
    {
        public string Email { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public Guid? ReliefStationId { get; set; }
        public bool IsStationHead { get; set; }
        public string? Notes { get; set; }
        public ModeratorStatus Status { get; set; } = ModeratorStatus.Inactive;
        public string? StatusReason { get; set; }
    }
}
