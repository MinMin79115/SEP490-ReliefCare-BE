namespace ReliefManagementSystem.Application.Features.Team.DTOs.Request
{
    public class AddMembersRequest
    {
        public List<Guid> VolunteerIds { get; set; } = new();
    }
}
