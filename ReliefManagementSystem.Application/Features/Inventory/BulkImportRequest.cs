using System.ComponentModel.DataAnnotations;

namespace ReliefManagementSystem.Application.Features.Inventory
{
    public class BulkImportRequest
    {
        [Required]
        [MinLength(1)]
        public List<ImportItemDto> Items { get; set; } = new();

        [MaxLength(500)]
        public string? Notes { get; set; }
    }

    public class ImportItemDto
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
