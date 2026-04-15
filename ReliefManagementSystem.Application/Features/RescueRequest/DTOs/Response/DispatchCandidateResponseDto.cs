namespace ReliefManagementSystem.Application.Features.RescueRequest.DTOs.Response
{
    public class DispatchCandidateResponseDto
    {
        public Guid RequestId { get; set; }
        public string? UserName { get; set; }
        public string? ReporterFullName { get; set; }
        public string? ReporterPhone { get; set; }
        public string RescueRequestType { get; set; } = null!;
        public string RescueRequestStatus { get; set; } = null!;
        public int? PriorityPoint { get; set; }
        public string? PriorityLevel { get; set; }
        public string? Address { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public Guid? AlreadyAssignedTeamId { get; set; }
        public bool IsInOtherActiveBatch { get; set; }
        public bool CanDispatch { get; set; }
        public string? DispatchBlockReason { get; set; }
    }

    public class PaginatedDispatchCandidatesResponseDto
    {
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;
        public List<DispatchCandidateResponseDto> Data { get; set; } = new();
    }
}
