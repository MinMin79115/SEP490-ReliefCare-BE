namespace ReliefManagementSystem.Application.Features.RescueRequest.DTOs.Response
{
    public class DispatchPreviewResponseDto
    {
        public Guid RequestId { get; set; }
        public Guid TeamId { get; set; }
        public bool Eligible { get; set; }
        public string RecommendedAction { get; set; } = null!;
        public bool WillPreemptCurrentInProgress { get; set; }
        public Guid? CurrentInProgressRequestId { get; set; }
        public Guid? CurrentInProgressBatchItemId { get; set; }
        public Guid? NewBatchItemId { get; set; }
        public int RecommendedQueueIndex { get; set; }
        public double? DistanceFromTeamKm { get; set; }
        public double? DistanceToCurrentInProgressKm { get; set; }
        public bool IsNearCurrentRoute { get; set; }
        public bool RequiresBacktrack { get; set; }
        public string? CurrentRoutePolyline { get; set; }
        public int? CurrentRouteDistanceMeters { get; set; }
        public int? CurrentRouteDurationSeconds { get; set; }
        public double? MinDistanceToCurrentRouteMeters { get; set; }
        public int? DetourMeters { get; set; }
        public int? DetourSeconds { get; set; }
        public string RescueRequestType { get; set; } = null!;
        public int? PriorityPoint { get; set; }
        public string? PriorityLevel { get; set; }
        public List<string> Reasons { get; set; } = new();
        public List<Guid> ProposedRequestIdsInOrder { get; set; } = new();
    }
}
