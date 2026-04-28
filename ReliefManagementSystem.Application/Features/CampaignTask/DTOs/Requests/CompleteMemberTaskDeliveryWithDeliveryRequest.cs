using System.ComponentModel.DataAnnotations;

namespace ReliefManagementSystem.Application.Features.CampaignTask.DTOs.Requests
{
    public class CompleteMemberTaskDeliveryWithDeliveryRequest
    {
        [Required]
        [MaxLength(1000)]
        public string ProofFileUrl { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? ProofContentType { get; set; }

        public string? ProofNote { get; set; }
        public string? DeliveryNote { get; set; }
    }
}
