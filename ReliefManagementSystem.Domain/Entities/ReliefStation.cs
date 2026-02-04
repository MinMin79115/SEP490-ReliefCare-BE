using ReliefManagementSystem.Domain.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Domain.Entities
{
    public class ReliefStation
    {
        public Guid ReliefStationId { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; } = null!;

        public Guid ManagerId { get; set; }
        public Guid LocationId { get; set; }
        public String? Address { get; set; }

        public String? ContactNumber { get; set; }

        public DateTime CreatedAt { get; set; }
            
        public DateTime UpdatedAt { get; set; }

        public Guid CreatedBy { get; set; }

        public double Longitude { get; set; }
        public double Latitude { get; set; }

        public bool IsActive { get; set; }

        public RelifeStationStatus Status { get; set; }

        public ApplicationUser Manager { get; set; } = null!;
        public Location Location { get; set; } = null!;
    }
}
