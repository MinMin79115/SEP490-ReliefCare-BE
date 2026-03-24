namespace ReliefManagementSystem.Application.Features.PriorityCriteria.DTOs.Request
{
    public class SearchPriorityCriteriaRequest
    {
        public string? Search { get; set; }
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
