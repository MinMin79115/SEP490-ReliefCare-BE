using System.ComponentModel.DataAnnotations;

namespace ReliefManagementSystem.Application.Features.Procurement.Dtos.Requests
{
    public class ReceiveProcurementOrderRequest
    {
        [MaxLength(500)]
        public string? ReceiveNote { get; set; }

        [Required]
        [MinLength(1)]
        public List<ReceiveProcurementOrderItemRequest> Items { get; set; } = new();
    }

    public class ReceiveProcurementOrderItemRequest
    {
        [Required]
        public Guid SupplyItemId { get; set; }

        [Range(1, int.MaxValue)]
        public int ReceivedQuantity { get; set; }

        [Range(0.01, double.MaxValue)]
        public decimal ActualUnitCost { get; set; }
    }
}
