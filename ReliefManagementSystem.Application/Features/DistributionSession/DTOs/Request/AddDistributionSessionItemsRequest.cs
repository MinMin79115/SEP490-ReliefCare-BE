using System.ComponentModel.DataAnnotations;

namespace ReliefManagementSystem.Application.Features.DistributionSession.DTOs.Request
{
    public class AddDistributionSessionItemsRequest
    {
        [Required]
        [MinLength(1)]
        public List<DistributionSessionItemInputDto> Items { get; set; } = new();
    }

    public class DistributionSessionItemInputDto
    {
        [Required]
        public Guid SupplyItemId { get; set; }

        public Guid? SupplyAllocationItemId { get; set; }

        [Range(typeof(decimal), "0.0001", "79228162514264337593543950335")]
        public decimal ReservedQuantity { get; set; }
    }
}
