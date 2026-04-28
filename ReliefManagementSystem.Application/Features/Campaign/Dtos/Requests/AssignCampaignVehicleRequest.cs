using ReliefManagementSystem.Domain.Enum;
using System.ComponentModel.DataAnnotations;

namespace ReliefManagementSystem.Application.Features.Campaign.Dtos.Requests
{
    public class AssignCampaignVehicleRequest
    {
        [Required]
        public Guid VehicleId { get; set; }

        [Required]
        public Guid CampaignTeamId { get; set; }

        public Guid? AssignedDriverId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public VehicleAssignmentStatus Status { get; set; } = VehicleAssignmentStatus.Approved;
        public string? Note { get; set; }
    }
}
