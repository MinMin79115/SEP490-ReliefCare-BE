using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Application.Features.Campaign.Dtos.Responses
{
    public class CampaignResponse
    {
        public Guid CampaignId { get; set; }
        public Guid LocationId { get; set; }
        public Guid CreatedBy { get; set; }

        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public double AreaRadiusKm { get; set; }
        public string? AddressDetail { get; set; }

        public CampaignStatus Status { get; set; }
        public CampaignType Type { get; set; }
        public CampaignCompletionRule CompletionRule { get; set; }
        public bool AllowOverTarget { get; set; }

        public DateTime CreatedAt { get; set; }

        public List<CampaignGoalResponse> Goals { get; set; } = new();
        public List<CampaignStationResponse> Stations { get; set; } = new();
    }
}
