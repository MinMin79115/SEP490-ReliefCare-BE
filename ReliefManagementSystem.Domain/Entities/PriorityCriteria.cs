using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Domain.Entities
{
    public class PriorityCriteria
    {
        public Guid PriorityCriteriaId { get; set; }

        public string Name { get; set; }
        public int Point { get; set; }
        public DisasterType DisasterType { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }

        public ICollection<RescueRequestPriority> RescueRequestPriorities { get; set; } = new List<RescueRequestPriority>();
    }
}

