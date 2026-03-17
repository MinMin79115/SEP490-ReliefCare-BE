using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Application.Features.StationJoinRequest.DTOs.Response
{
    public class StationJoinRequestResponse
    {
        public Guid StationJoinRequestId { get; set; }

        public Guid TeamId { get; set; }
        public string TeamName { get; set; } = null!;

        public Guid ReliefStationId { get; set; }
        public string ReliefStationName { get; set; } = null!;

        public Guid RequestedByLeaderId { get; set; }
        public string RequestedByLeaderName { get; set; } = null!;

        public StationJoinRequestStatus Status { get; set; }
        public string? Description { get; set; }
        public string? RejectionReason { get; set; }
        public string? ReviewNote { get; set; }

        public DateTime RequestedAt { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public Guid? ReviewedByModeratorId { get; set; }
        public string? ReviewedByModeratorName { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime? RejectedAt { get; set; }
        public DateTime? CancelledAt { get; set; }
    }
}
