using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Application.Features.Vehicle.DTOs.Request
{
    public class UpdateVehicleRequest
    {
        [Required(ErrorMessage = "Vehicle Type ID is required")]
        public Guid VehicleTypeId { get; set; }

        [Required(ErrorMessage = "License Plate is required")]
        [StringLength(20, ErrorMessage = "License Plate cannot exceed 20 characters")]
        public string LicensePlate { get; set; } = string.Empty;

        public Guid? TeamId { get; set; }

        [Range(1, 2, ErrorMessage = "Status must be 1 (Free) or 2 (Busy)")]
        public int Status { get; set; }
    }
}
