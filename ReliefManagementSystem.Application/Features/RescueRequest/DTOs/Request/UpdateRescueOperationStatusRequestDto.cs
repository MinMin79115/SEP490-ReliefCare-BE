using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Application.Features.RescueRequest.DTOs.Request
{
    public class UpdateRescueOperationStatusRequestDto
    {
        public RescueOperationStatus Status { get; set; }
        public string? Note { get; set; }
    }
}
