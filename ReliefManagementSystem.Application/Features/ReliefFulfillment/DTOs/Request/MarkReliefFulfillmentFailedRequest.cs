using System.ComponentModel.DataAnnotations;

namespace ReliefManagementSystem.Application.Features.ReliefFulfillment.DTOs.Request
{
    public class MarkReliefFulfillmentFailedRequest
    {
        [MaxLength(1000)]
        public string? Note { get; set; }
    }
}
