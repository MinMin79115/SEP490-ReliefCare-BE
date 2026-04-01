namespace ReliefManagementSystem.Application.Features.DistributionSession.DTOs.Request
{
    public class SearchDistributionSessionRequest
    {
        public string? Search { get; set; }
        public int? StatusFilter { get; set; }
        public Guid? CampaignId { get; set; }
        public Guid? ReliefStationId { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
