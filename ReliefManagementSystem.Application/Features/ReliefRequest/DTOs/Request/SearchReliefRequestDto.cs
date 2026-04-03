namespace ReliefManagementSystem.Application.Features.ReliefRequest.DTOs.Request
{
    public class SearchReliefRequestDto
    {
        public string? Search { get; set; }

        public int? StatusFilter { get; set; }

        public Guid? AssignedStationId { get; set; }

        public Guid? CampaignId { get; set; }

        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 10;
    }
}
