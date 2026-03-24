using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Application.Features.ReliefStation.DTOs.Response
{
    /// <summary>Summary response for list views.</summary>
    public class ReliefStationResponse
    {
        public Guid ReliefStationId { get; set; }
        public string Name { get; set; } = null!;
        public string ModeratorName { get; set; } = null!;
        public string? Address { get; set; }
        public string? ContactNumber { get; set; }
        public double Longitude { get; set; }
        public double Latitude { get; set; }
        public double CoverageRadiusKm { get; set; }
        public ReliefStationStatus Status { get; set; }
        public ReliefStationLevel Level { get; set; }
        public Guid LocationId { get; set; }
        public string LocationName { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class ReliefStationDetailResponse : ReliefStationResponse
    {
        public int TotalInventories { get; set; }
        public IReadOnlyList<StationTeamResponse> Teams { get; set; } = [];
    }

    /// <summary>Response for a team assigned to a station.</summary>
    public class StationTeamResponse
    {
        public Guid AssignmentId { get; set; }
        public Guid TeamId { get; set; }
        public string TeamName { get; set; } = null!;
        public ReliefTeamAssignmentStatus Status { get; set; }
        public string StatusName => Status.ToString();
        public bool IsActive { get; set; }
        public string? Description { get; set; }
        public string? RejectionReason { get; set; }
        public DateTime? JoinedAt { get; set; }
        public DateTime? TransferredAt { get; set; }
    }
}
