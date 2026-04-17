using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Application.Features.VehicleType.DTOs.Request
{
    public class UpdateVehicleTypeRequest
    {
        [Required(ErrorMessage = "Type Name is required")]
        [StringLength(100, ErrorMessage = "Type Name cannot exceed 100 characters")]
        public string TypeName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Default Capacity is required")]
        public int DefaultCapacity { get; set; }

        [Required(ErrorMessage = "Capacity Kind is required")]
        [Range(1, 2, ErrorMessage = "Capacity Kind must be 1 (CargoWeight) or 2 (PassengerCount)")]
        public CapacityKind CapacityKind { get; set; }

        [Required(ErrorMessage = "Capacity Unit is required")]
        [StringLength(20, ErrorMessage = "Capacity Unit cannot exceed 20 characters")]
        public string CapacityUnit { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }
    }
}
