using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Application.Features.User
{
    public class UpdateModeratorAccountRequest
    {
        public string? Email { get; set; }
        public string? UserName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? FullName { get; set; }
        public Guid? ReliefStationId { get; set; }
        public bool ClearReliefStation { get; set; }
        public bool? IsStationHead { get; set; }
        public string? Notes { get; set; }
        public ModeratorStatus? Status { get; set; }
        public string? StatusReason { get; set; }
    }
}
