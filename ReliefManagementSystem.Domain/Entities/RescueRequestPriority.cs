using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Domain.Entities
{
    public class RescueRequestPriority
    {
        public Guid RescueRequestId { get; set; }
        public Guid PriorityCriteriaId { get; set; }

        public int AppliedPoint { get; set; }
        public string Status { get; set; }

        public RescueRequest RescueRequest { get; set; }
        public PriorityCriteria PriorityCriteria { get; set; }
    }
}
