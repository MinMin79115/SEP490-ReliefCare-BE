using System.ComponentModel.DataAnnotations;

namespace ReliefManagementSystem.Application.Features.Procurement.Dtos.Requests
{
    public class CreateProcurementOrderRequest
    {
        [Required]
        public Guid CampaignId { get; set; }

        [Required]
        public Guid DestinationInventoryId { get; set; }

        [MaxLength(255)]
        public string? SupplierName { get; set; }

        [MaxLength(100)]
        public string? SupplierContact { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        [Required]
        [MinLength(1)]
        public List<CreateProcurementOrderItemRequest> Items { get; set; } = new();
    }

    public class CreateProcurementOrderItemRequest
    {
        [Required]
        public Guid SupplyItemId { get; set; }

        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        [Range(0.01, double.MaxValue)]
        public decimal UnitCost { get; set; }
    }
}
