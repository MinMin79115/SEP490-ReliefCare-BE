using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Application.Features.ReliefStation.Dtos
{
    public class CreateReliefStationRequest
    {
        public string Name { get; set; } = null!;
        public Guid LocationId { get; set; }
        public string? Address { get; set; }
        public string? ContactNumber { get; set; }

        public Guid? ManagerId { get; set; }
        public Guid? ParentReliefStationId { get; set; }

        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
