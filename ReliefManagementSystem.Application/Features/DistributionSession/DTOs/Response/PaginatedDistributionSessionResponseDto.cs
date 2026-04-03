namespace ReliefManagementSystem.Application.Features.DistributionSession.DTOs.Response
{
    public class PaginatedDistributionSessionResponseDto
    {
        public List<DistributionSessionResponseDto> Data { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (TotalCount + PageSize - 1) / PageSize;
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;
    }
}
