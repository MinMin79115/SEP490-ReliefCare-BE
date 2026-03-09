using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Application.Features.ReliefStation.Dtos
{
    public class CreateProvincialReliefStationRequest
    {
        
        public Guid LocationId { get; set; }

        public string Name { get; set; } = null!;

        public string? Address { get; set; }

        public string? ContactNumber { get; set; }

        public double Longitude { get; set; }

        public double Latitude { get; set; }
    }
}
