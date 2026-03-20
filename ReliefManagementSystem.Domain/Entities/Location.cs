using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Domain.Entities
{
    public class Location
    {
        public Guid LocationId { get; set; }   

        public Guid? ParentId { get; set; }
        public string Name { get; set; }
        public decimal PopulationDensity { get; set; } 
        public decimal Area { get; set; }              
        public long Population { get; set; }
        public string NormalizedName { get; set; } = null!;
        public string Path { get; set; } = null!;

        public string FullName { get; set; } = null!;
        public LocationLevel Level { get; set; }

        public int Status { get; set; }

        
        public Location Parent { get; set; }
        public ICollection<Location> Children { get; set; }
        public ICollection<Request> Requests { get; set; }
            = new List<Request>();
    }
}
