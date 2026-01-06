using ReliefManagementSystem.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Domain.Entities
{
    public class Team
    {
        public int TeamId { get; set; }

        public string Name { get; set; }

        public int LeaderId { get; set; }
        public ApplicationUser Leader { get; set; }

        public TeamStatus Status { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
