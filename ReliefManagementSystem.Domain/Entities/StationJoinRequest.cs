using ReliefManagementSystem.Domain.Enum;
using System.ComponentModel.DataAnnotations;

namespace ReliefManagementSystem.Domain.Entities
{
    public class StationJoinRequest
    {
        public Guid StationJoinRequestId { get; set; }

        public Guid TeamId { get; set; }
        public Team Team { get; set; } = null!;

        public Guid ReliefStationId { get; set; }
        public ReliefStation ReliefStation { get; set; } = null!;

        public Guid RequestedByLeaderId { get; set; }
        public ApplicationUser RequestedByLeader { get; set; } = null!;

        public StationJoinRequestStatus Status { get; set; } = StationJoinRequestStatus.Pending;

        [MaxLength(1000)]
        public string? Description { get; set; }

        [MaxLength(1000)]
        public string? RejectionReason { get; set; }

        [MaxLength(1000)]
        public string? ReviewNote { get; set; }

        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ReviewedAt { get; set; }
        public Guid? ReviewedByModeratorId { get; set; }
        public ApplicationUser? ReviewedByModerator { get; set; }

        public DateTime? ApprovedAt { get; set; }
        public DateTime? RejectedAt { get; set; }
        public DateTime? CancelledAt { get; set; }
    }
}
