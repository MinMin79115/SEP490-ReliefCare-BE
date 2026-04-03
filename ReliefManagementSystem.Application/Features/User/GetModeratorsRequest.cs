namespace ReliefManagementSystem.Application.Features.User
{
    public class GetModeratorsRequest
    {
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? Search { get; set; }
        public bool? IsBanned { get; set; }
    }
}
