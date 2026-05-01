namespace ReliefManagementSystem.Application.Features.Campaign.Dtos.Requests
{
    public class AssignCampaignVehicleDriverRequest
    {
        public Guid AssignedDriverId { get; set; }
        public string? Note { get; set; }
    }
}
