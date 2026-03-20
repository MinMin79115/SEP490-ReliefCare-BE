using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Application.Features.SupplyItem.DTOs.Response
{
    /// <summary>
    /// Response model for a supply item (list / create / update).
    /// </summary>
    public class SupplyItemResponse
    {
        public Guid SupplyItemId { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public string? IconUrl { get; set; }
        public SupplyCategory Category { get; set; }
        public string CategoryName => Category.ToString();
        public string Unit { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
