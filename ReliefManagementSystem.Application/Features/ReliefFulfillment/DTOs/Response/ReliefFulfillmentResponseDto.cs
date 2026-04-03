using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Application.Features.ReliefFulfillment.DTOs.Response
{
    public class ReliefFulfillmentResponseDto
    {
        public Guid ReliefFulfillmentId { get; set; }
        public Guid ReliefRequestId { get; set; }
        public Guid DistributionSessionId { get; set; }
        public int WaveNumber { get; set; }
        public DistributionSessionMode Mode { get; set; }
        public ReliefFulfillmentStatus Status { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public string? RecipientName { get; set; }
        public string? RecipientPhone { get; set; }
        public string? DeliveryNote { get; set; }
        public string? ProofImageUrl { get; set; }
        public Guid? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<ReliefFulfillmentItemResponseDto> Items { get; set; } = new();
    }

    public class ReliefFulfillmentItemResponseDto
    {
        public Guid ReliefFulfillmentItemId { get; set; }
        public Guid SupplyItemId { get; set; }
        public string SupplyItemName { get; set; } = string.Empty;
        public ReliefNeedType? NeedCategory { get; set; }
        public decimal PlannedQuantity { get; set; }
        public decimal ActualDeliveredQuantity { get; set; }
        public string? Note { get; set; }
    }
}
