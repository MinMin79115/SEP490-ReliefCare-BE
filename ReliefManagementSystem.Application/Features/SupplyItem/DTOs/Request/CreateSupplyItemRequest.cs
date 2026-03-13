using ReliefManagementSystem.Domain.Enum;
using System.ComponentModel.DataAnnotations;

namespace ReliefManagementSystem.Application.Features.SupplyItem.DTOs.Request
{
    /// <summary>
    /// Request model for creating a new supply item.
    /// </summary>
    public class CreateSupplyItemRequest
    {
        /// <summary>
        /// Display name of the supply item. Must be unique.
        /// </summary>
        [Required(ErrorMessage = "Name is required.")]
        [MaxLength(200, ErrorMessage = "Name must not exceed 200 characters.")]
        public string Name { get; set; } = null!;

        /// <summary>
        /// Optional description of the item.
        /// </summary>
        [MaxLength(500, ErrorMessage = "Description must not exceed 500 characters.")]
        public string? Description { get; set; }

        /// <summary>
        /// Optional URL of the icon image for this supply item.
        /// Accepts any valid URL (https recommended) or relative path.
        /// </summary>
        [MaxLength(500, ErrorMessage = "IconUrl must not exceed 500 characters.")]
        [Url(ErrorMessage = "IconUrl must be a valid URL.")]
        public string? IconUrl { get; set; }

        /// <summary>
        /// Category of the supply item.
        /// </summary>
        [Required(ErrorMessage = "Category is required.")]
        public SupplyCategory Category { get; set; }

        /// <summary>
        /// Unit of measurement, e.g. "Thùng", "Cái", "Hộp", "Bộ".
        /// </summary>
        [Required(ErrorMessage = "Unit is required.")]
        [MaxLength(50, ErrorMessage = "Unit must not exceed 50 characters.")]
        public string Unit { get; set; } = null!;
    }
}
