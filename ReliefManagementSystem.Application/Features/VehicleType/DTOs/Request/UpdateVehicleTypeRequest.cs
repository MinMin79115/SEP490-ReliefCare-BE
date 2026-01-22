using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Application.Features.VehicleType.DTOs.Request
{
    public class UpdateVehicleTypeRequest
    {
        [Required(ErrorMessage = "Type Name is required")]
        [StringLength(100, ErrorMessage = "Type Name cannot exceed 100 characters")]
        public string TypeName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Default Capacity is required")]
        public int DefaultCapacity { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }
    }
}
