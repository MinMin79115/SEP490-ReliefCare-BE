namespace ReliefManagementSystem.Application.Features.Team.DTOs.Response
{
    public class AddMembersResponse
    {
        public Guid TeamId { get; set; }
        public int TotalRequested { get; set; }
        public int SuccessCount { get; set; }
        public int FailedCount { get; set; }
        public List<TeamMemberResponse> AddedMembers { get; set; } = new();
        public List<AddMemberFailureItem> FailedMembers { get; set; } = new();
    }

    public class AddMemberFailureItem
    {
        public Guid VolunteerId { get; set; }
        public string Reason { get; set; } = null!;
    }
}
