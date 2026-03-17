namespace ReliefManagementSystem.Application.Features.StationJoinRequest.DTOs.Request
{
    public class CreateStationJoinRequestRequest
    {
        public Guid TeamId { get; set; }
        public Guid ReliefStationId { get; set; }
        public string? Description { get; set; }
    }
}
