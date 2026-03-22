namespace ReliefManagementSystem.Application.Features.Team.DTOs.Request
{
    public class GetTeamsInStationRequest
    {
        public Guid ReliefStationId { get; set; }
        public string? Search { get; set; }
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
