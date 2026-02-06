using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public int Status { get; set; }

        
        public Location Parent { get; set; }
        public ICollection<Location> Children { get; set; }
    }
}
