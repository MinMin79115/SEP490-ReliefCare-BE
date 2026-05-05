namespace ReliefManagementSystem.Application.Features.Campaign.Dtos.Requests
{
    public class HandoffCampaignVehicleRequest
    {
        public Guid ToVolunteerProfileId { get; set; }
        public string? Note { get; set; }
    }
}
