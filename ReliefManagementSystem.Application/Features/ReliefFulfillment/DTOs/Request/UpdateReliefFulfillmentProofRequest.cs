using System.ComponentModel.DataAnnotations;

namespace ReliefManagementSystem.Application.Features.ReliefFulfillment.DTOs.Request
{
    public class UpdateReliefFulfillmentProofRequest
    {
        [MaxLength(1000)]
        public string? ProofImageUrl { get; set; }

        [MaxLength(1000)]
        public string? DeliveryNote { get; set; }
    }
}
