using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Application.Features.Location.Dtos
{
    public class LocationTreeDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int Level { get; set; }

        public List<LocationTreeDto> Children { get; set; } = new();
    }
}
