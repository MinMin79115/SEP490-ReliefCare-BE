using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Application.Features.User
{
    public class ModeratorProfileResponse
    {
        public Guid Id { get; set; }
        public string? DisplayName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? PictureUrl { get; set; }

        public bool IsBanned { get; set; }
        public DateTimeOffset? LockoutEnd { get; set; }
        public string? BanReason { get; set; }

        public ModeratorStatus? ModeratorStatus { get; set; }
        public bool IsStationHead { get; set; }
        public bool IsManagingStation { get; set; }
        public Guid? ReliefStationId { get; set; }
        public string? ReliefStationName { get; set; }
        public DateTime AppointedAt { get; set; }
        public string? Notes { get; set; }
        public string? StatusReason { get; set; }
    }
}
