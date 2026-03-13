using ReliefManagementSystem.Domain.Enum;
using System.ComponentModel.DataAnnotations;

namespace ReliefManagementSystem.Application.Features.SupplyItem.DTOs.Request
{
    /// <summary>
    /// Request model for updating an existing supply item.
    /// </summary>
    public class UpdateSupplyItemRequest
    {
        /// <summary>
        /// Updated display name. Must be unique.
        /// </summary>
        [Required(ErrorMessage = "Name is required.")]
        [MaxLength(200, ErrorMessage = "Name must not exceed 200 characters.")]
        public string Name { get; set; } = null!;

        /// <summary>
        /// Updated description.
        /// </summary>
        [MaxLength(500, ErrorMessage = "Description must not exceed 500 characters.")]
        public string? Description { get; set; }

        /// <summary>
        /// Updated icon URL. Pass null to clear the existing icon.
        /// </summary>
        [MaxLength(500, ErrorMessage = "IconUrl must not exceed 500 characters.")]
        [Url(ErrorMessage = "IconUrl must be a valid URL.")]
        public string? IconUrl { get; set; }

        /// <summary>
        /// Updated category.
        /// </summary>
        [Required(ErrorMessage = "Category is required.")]
        public SupplyCategory Category { get; set; }

        /// <summary>
        /// Updated unit of measurement.
        /// </summary>
        [Required(ErrorMessage = "Unit is required.")]
        [MaxLength(50, ErrorMessage = "Unit must not exceed 50 characters.")]
        public string Unit { get; set; } = null!;
    }
}
