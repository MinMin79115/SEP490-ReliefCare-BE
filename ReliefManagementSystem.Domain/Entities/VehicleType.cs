using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Domain.Entities
{
    public class VehicleType
    {
        public Guid VehicleTypeId { get; set; } = Guid.NewGuid();

        public string TypeName { get; set; } = string.Empty;

        public int DefaultCapacity { get; set; }

        public string? Description { get; set; }

        public bool IsDeleted { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation property
        public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
    }
}
