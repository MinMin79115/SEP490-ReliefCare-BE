using ReliefManagementSystem.Domain.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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

         public ReliefTeamAssignmentStatus Status { get; set; }

         [MaxLength(1000)]
         public string? Description { get; set; }

         [MaxLength(1000)]
         public string? RejectionReason { get; set; }

         public DateTime? JoinedAt { get; set; }
         public DateTime? RemovedAt{ get; set; }

    }
}
