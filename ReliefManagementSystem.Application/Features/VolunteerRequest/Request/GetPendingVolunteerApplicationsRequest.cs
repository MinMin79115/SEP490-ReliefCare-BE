using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Application.Features.VolunteerRequest.Request
{
    public class GetPendingVolunteerApplicationsRequest
    {
        public string? Search { get; set; }
        public VerificationStatus? VerificationStatus { get; set; }
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
