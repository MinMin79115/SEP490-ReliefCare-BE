using ReliefManagementSystem.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Domain.Entities
{
    public class ReliefStationTeam
    {
       public Guid ReliefStationTeamId { get; set; }

       public Guid ReliefStationId { get; set; }
       public ReliefStation ReliefStation { get; set; } = null!;

        public Guid TeamId { get; set; }
        public Team Team { get; set; } = null!;

        public bool IsActive { get; set; } = true;

        public ReliefTeamAssignmentStatus Status { get; set; }
    }
}
