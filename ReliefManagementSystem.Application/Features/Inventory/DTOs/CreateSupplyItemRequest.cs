using System.ComponentModel.DataAnnotations;

namespace ReliefManagementSystem.Application.Features.Inventory.DTOs
{
    public class CreateSupplyItemRequest
    {
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = null!;

        [MaxLength(1000)]
        public string? Description { get; set; }

        [MaxLength(500)]
        public string? IconUrl { get; set; }

        [Required]
        public Domain.Enum.SupplyCategory Category { get; set; }

        [Required]
        [MaxLength(50)]
        public string Unit { get; set; } = null!;

        [Range(0, int.MaxValue)]
        public int CurrentQuantity { get; set; } = 0;

        [Required]
        [Range(0, int.MaxValue)]
        public int MinimumStockLevel { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int MaximumStockLevel { get; set; }
    }
}
