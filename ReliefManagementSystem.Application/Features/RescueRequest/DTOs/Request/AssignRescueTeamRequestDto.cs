using System.ComponentModel.DataAnnotations;

namespace ReliefManagementSystem.Application.Features.RescueRequest.DTOs.Request
{
    public class AssignRescueTeamRequestDto
    {
        [Required]
        public Guid TeamId { get; set; }

        public Guid? VehicleId { get; set; }

        public List<Guid>? VehicleIds { get; set; }

        public List<AssignRescueSupplyItemDto>? Supplies { get; set; }

        public string? Note { get; set; }
    }

    public class AssignRescueSupplyItemDto
    {
        public Guid SourceInventoryId { get; set; }
        public Guid SupplyItemId { get; set; }
        public int Quantity { get; set; }
        public string? Unit { get; set; }
        public string? Notes { get; set; }
    }
}
