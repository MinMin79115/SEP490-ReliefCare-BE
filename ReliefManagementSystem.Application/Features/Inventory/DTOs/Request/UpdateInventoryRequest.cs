using ReliefManagementSystem.Domain.Enum;
using System.ComponentModel.DataAnnotations;

namespace ReliefManagementSystem.Application.Features.Inventory.DTOs.Request
{
    /// <summary>
    /// Request model to update an existing inventory.
    /// </summary>
    public class UpdateInventoryRequest
    {
        /// <summary>Updated inventory level.</summary>
        [Required(ErrorMessage = "Level is required.")]
        public InventoryLevel Level { get; set; }

        /// <summary>Updated status.</summary>
        [Required(ErrorMessage = "Status is required.")]
        public EntityStatus Status { get; set; }
    }
}
