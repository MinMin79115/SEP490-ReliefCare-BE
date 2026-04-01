using ReliefManagementSystem.Domain.Enum;
using System.ComponentModel.DataAnnotations;

namespace ReliefManagementSystem.Application.Features.ReliefFulfillment.DTOs.Request
{
    public class CreateReliefFulfillmentRequest
    {
        [Required]
        public Guid ReliefRequestId { get; set; }

        [MaxLength(200)]
        public string? RecipientName { get; set; }

        [MaxLength(50)]
        public string? RecipientPhone { get; set; }

        [MaxLength(1000)]
        public string? DeliveryNote { get; set; }

        [MaxLength(1000)]
        public string? ProofImageUrl { get; set; }

        public DateTime? DeliveredAt { get; set; }

        [Required]
        [MinLength(1)]
        public List<CreateReliefFulfillmentItemRequest> Items { get; set; } = new();
    }

    public class CreateReliefFulfillmentItemRequest
    {
        [Required]
        public Guid SupplyItemId { get; set; }

        public ReliefNeedType? NeedCategory { get; set; }

        [Range(typeof(decimal), "0", "79228162514264337593543950335")]
        public decimal PlannedQuantity { get; set; }

        [Range(typeof(decimal), "0.0001", "79228162514264337593543950335")]
        public decimal ActualDeliveredQuantity { get; set; }

        [MaxLength(500)]
        public string? Note { get; set; }
    }
}
