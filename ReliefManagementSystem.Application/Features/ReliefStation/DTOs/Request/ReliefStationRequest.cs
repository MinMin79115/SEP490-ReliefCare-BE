using ReliefManagementSystem.Domain.Enum;
using System.ComponentModel.DataAnnotations;

namespace ReliefManagementSystem.Application.Features.ReliefStation.DTOs.Request
{
    /// <summary>Request model to create a new relief station.</summary>
    public class CreateReliefStationRequest
    {
        [Required(ErrorMessage = "Name is required.")]
        [MaxLength(255)]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "LocationId is required.")]
        public Guid LocationId { get; set; }

        [Required(ErrorMessage = "ManagerId is required.")]
        public Guid ManagerId { get; set; }

        [MaxLength(500)]
        public string? Address { get; set; }

        [MaxLength(20)]
        public string? ContactNumber { get; set; }

        public double Longitude { get; set; }
        public double Latitude { get; set; }

        public RelifeStationStatus Status { get; set; } = RelifeStationStatus.Draft;
    }

    /// <summary>Request model to update an existing relief station.</summary>
    public class UpdateReliefStationRequest
    {
        [Required(ErrorMessage = "Name is required.")]
        [MaxLength(255)]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "LocationId is required.")]
        public Guid LocationId { get; set; }

        [Required(ErrorMessage = "ManagerId is required.")]
        public Guid ManagerId { get; set; }

        [MaxLength(500)]
        public string? Address { get; set; }

        [MaxLength(20)]
        public string? ContactNumber { get; set; }

        public double Longitude { get; set; }
        public double Latitude { get; set; }

        public RelifeStationStatus Status { get; set; }
    }

    /// <summary>Request model to assign a team to a relief station.</summary>
    public class AssignTeamRequest
    {
        [Required(ErrorMessage = "TeamId is required.")]
        public Guid TeamId { get; set; }
    }

    /// <summary>Request model to update the assignment status of a team at a station.</summary>
    public class UpdateTeamAssignmentRequest
    {
        [Required(ErrorMessage = "Status is required.")]
        public ReliefTeamAssignmentStatus Status { get; set; }
    }
}
