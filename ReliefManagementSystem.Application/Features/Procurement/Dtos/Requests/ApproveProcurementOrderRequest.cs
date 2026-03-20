using System.ComponentModel.DataAnnotations;

namespace ReliefManagementSystem.Application.Features.Procurement.Dtos.Requests
{
    public class ApproveProcurementOrderRequest
    {
        [MaxLength(500)]
        public string? ApprovalNote { get; set; }
    }
}
