namespace ReliefManagementSystem.Application.Features.RescueRequest.DTOs.Request
{
    public class SearchRescueRequestDto
    {
        public string? Search { get; set; }

        public int? StatusFilter { get; set; }

        public int? VerificationStatus { get; set; }

        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 10;
    }
}
