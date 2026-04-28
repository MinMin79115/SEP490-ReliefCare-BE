using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Application.Features.Campaign.Dtos.Requests
{
    public class UpdateCampaignVehicleAssignmentRequest
    {
        public Guid? CampaignTeamId { get; set; }
        public Guid? AssignedDriverId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public VehicleAssignmentStatus? Status { get; set; }
        public string? Note { get; set; }
    }
}
