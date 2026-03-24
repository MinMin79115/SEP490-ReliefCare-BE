namespace ReliefManagementSystem.Application.Features.Skill.Dtos
{
    public class SearchSkillRequest
    {
        public int PageIndex { get; set; } = 1;

        public int PageSize { get; set; } = 10;

        public string? Search { get; set; }
    }
}
