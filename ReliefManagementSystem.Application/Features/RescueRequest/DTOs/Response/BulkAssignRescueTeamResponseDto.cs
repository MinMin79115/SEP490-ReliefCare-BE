namespace ReliefManagementSystem.Application.Features.RescueRequest.DTOs.Response
{
    public class BulkAssignRescueTeamResponseDto
    {
        public Guid TeamId { get; set; }
        public int TotalRequested { get; set; }
        public int SuccessCount { get; set; }
        public int FailedCount { get; set; }
        public List<Guid> SuccessRequestIds { get; set; } = new();
        public List<BulkAssignFailureItemDto> Failures { get; set; } = new();
    }

    public class BulkAssignFailureItemDto
    {
        public Guid RequestId { get; set; }
        public string Reason { get; set; } = string.Empty;
    }
}
