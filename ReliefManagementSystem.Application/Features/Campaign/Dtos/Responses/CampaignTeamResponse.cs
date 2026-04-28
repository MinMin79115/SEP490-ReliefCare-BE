using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Application.Features.Campaign.Dtos.Responses
{
    public class CampaignTeamResponse
    {
        public Guid CampaignTeamId { get; set; }
        public Guid CampaignId { get; set; }
        public Guid TeamId { get; set; }
        public string TeamName { get; set; } = string.Empty;
        public CampaignTeamRole Role { get; set; }
        public CampaignTeamStatus Status { get; set; }
        public DateTime AssignedAt { get; set; }
        public int MemberCount { get; set; }
        public List<CampaignAssignedVehicleResponse> Vehicles { get; set; } = [];
    }

    public class CampaignAssignedVehicleResponse
    {
        public Guid CampaignVehicleId { get; set; }
        public Guid VehicleId { get; set; }
        public string LicensePlate { get; set; } = string.Empty;
        public string VehicleTypeName { get; set; } = string.Empty;
        public Guid? CampaignTeamId { get; set; }
        public Guid? AssignedDriverId { get; set; }
        public VehicleAssignmentStatus Status { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Note { get; set; }
    }
}
