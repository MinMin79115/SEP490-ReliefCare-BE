using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Application.Features.VehicleType.DTOs.Response
{
    public class VehicleTypeDetailResponse
    {
        public Guid VehicleTypeId { get; set; }
        public string TypeName { get; set; } = string.Empty;
        public int DefaultCapacity { get; set; }
        public int CapacityKind { get; set; }
        public string CapacityKindName { get; set; } = string.Empty;
        public string CapacityUnit { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int TotalVehicles { get; set; }
        public int FreeVehicles { get; set; }
        public int BusyVehicles { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
