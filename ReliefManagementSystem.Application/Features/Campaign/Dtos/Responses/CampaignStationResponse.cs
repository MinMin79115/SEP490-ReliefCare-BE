namespace ReliefManagementSystem.Application.Features.Campaign.Dtos.Responses
{
    public class CampaignStationResponse
    {
        public Guid ReliefStationId { get; set; }
        public string ReliefStationName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime AssignedAt { get; set; }
    }
}
