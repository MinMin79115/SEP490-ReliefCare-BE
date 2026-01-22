using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Application.Features.VehicleType.DTOs.Response
{
    public class VehicleTypeResponse
    {
        public Guid VehicleTypeId { get; set; }
        public string TypeName { get; set; } = string.Empty;
        public int DefaultCapacity { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
