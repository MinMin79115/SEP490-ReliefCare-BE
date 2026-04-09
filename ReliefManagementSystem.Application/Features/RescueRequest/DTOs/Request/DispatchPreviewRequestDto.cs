namespace ReliefManagementSystem.Application.Features.RescueRequest.DTOs.Request
{
    public class DispatchPreviewRequestDto
    {
        public Guid TeamId { get; set; }
        public bool AllowPreempt { get; set; } = true;
        public double NormalNearRouteThresholdKm { get; set; } = 2.0;
        public double EmergencyNearRouteThresholdKm { get; set; } = 3.0;
    }
}
