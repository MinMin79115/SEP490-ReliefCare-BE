using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Application.Features.Campaign.Dtos.Requests
{
    public class CampaignListQueryRequest
    {
        public string? Keyword { get; set; }
        public CampaignStatus? Status { get; set; }
        public CampaignType? Type { get; set; }
        public Guid? LocationId { get; set; }
        public Guid? ReliefStationId { get; set; }
        public bool ForVolunteerRegistration { get; set; }
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
