using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Application.Features.RescueRequest.DTOs.Response
{
    public class RequestVerificationDto
    {
        public Guid RequestVerificationId { get; set; }
        public RequestVerificationStatus Status { get; set; }
        public VerificationMethod Method { get; set; }
        public string? Note { get; set; }
        public string? Reason { get; set; }
        public Guid? VerifiedBy { get; set; }
        public DateTime? VerifiedAt { get; set; }
    }
}
