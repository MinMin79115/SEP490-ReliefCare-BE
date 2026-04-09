namespace ReliefManagementSystem.Application.Features.RescueRequest.DTOs.Request
{
    public class GetDispatchCandidatesRequestDto
    {
        public Guid TeamId { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? Search { get; set; }
    }
}
