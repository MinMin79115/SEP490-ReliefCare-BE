namespace ReliefManagementSystem.Application.Features.RescueRequest.DTOs.Request
{
    public class SmartAssignRescueTeamRequestDto : DispatchPreviewRequestDto
    {
        public Guid? VehicleId { get; set; }

        public List<Guid>? VehicleIds { get; set; }

        public List<AssignRescueSupplyItemDto>? Supplies { get; set; }

        public string? Note { get; set; }
    }
}
