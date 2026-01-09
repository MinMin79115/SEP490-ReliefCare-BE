using System.ComponentModel.DataAnnotations;

namespace ReliefManagementSystem.Application.Features.Inventory
{
    public class BulkExportRequest
    {
        [Required]
        [MinLength(1)]
        public List<ExportItemDto> Items { get; set; } = new();

        [MaxLength(500)]
        public string? Notes { get; set; }

        [MaxLength(200)]
        public string? RecipientInfo { get; set; }
    }

    public class ExportItemDto
    {
        [Required]
        public Guid SupplyItemId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; }

        [MaxLength(200)]
        public string? Notes { get; set; }
    }
}
