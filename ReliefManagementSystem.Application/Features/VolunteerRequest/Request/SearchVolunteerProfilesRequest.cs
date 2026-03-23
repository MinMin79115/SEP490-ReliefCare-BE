namespace ReliefManagementSystem.Application.Features.VolunteerRequest.Request
{
    public class SearchVolunteerProfilesRequest
    {
        public int PageIndex { get; set; } = 1;

        public int PageSize { get; set; } = 10;

        public string? Search { get; set; }
    }
}
