using ReliefManagementSystem.Domain.Enum;
using System.ComponentModel.DataAnnotations;

namespace ReliefManagementSystem.Application.Features.Inventory.DTOs.Request
{
    /// <summary>
    /// Request model to create a new inventory for a relief station.
    /// </summary>
    public class CreateInventoryRequest
    {
        /// <summary>The relief station this inventory belongs to.</summary>
        [Required(ErrorMessage = "ReliefStationId is required.")]
        public Guid ReliefStationId { get; set; }

        /// <summary>Inventory level: Regional (1) or Provincial (2).</summary>
        [Required(ErrorMessage = "Level is required.")]
        public InventoryLevel Level { get; set; }

        /// <summary>Initial status. Defaults to Active if not provided.</summary>
        public EntityStatus Status { get; set; } = EntityStatus.Active;
    }
}
