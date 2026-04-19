using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Application.Features.Vehicle.DTOs.Response
{
    public class VehicleResponse
    {
        public Guid VehicleId { get; set; }
        public Guid VehicleTypeId { get; set; }
        public string VehicleTypeName { get; set; } = string.Empty;
        public string LicensePlate { get; set; } = string.Empty;
        public Guid CreatedBy { get; set; }
        public string CreatorName { get; set; } = string.Empty;
        public Guid? ReliefStationId { get; set; }
        public string? ReliefStationName { get; set; }
        public Guid? TeamId { get; set; }
        public string? TeamName { get; set; }
        public Guid? CurrentOperationId { get; set; }
        public Guid? CurrentUsingTeamId { get; set; }
        public string? CurrentUsingTeamName { get; set; }
        public int Status { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
