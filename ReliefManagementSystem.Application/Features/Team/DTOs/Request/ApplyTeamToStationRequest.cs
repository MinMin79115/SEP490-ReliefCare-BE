namespace ReliefManagementSystem.Application.Features.Team.DTOs.Request
{
    public class ApplyTeamToStationRequest
    {
        public Guid ReliefStationId { get; set; }

        public string? Description { get; set; }
    }
}
