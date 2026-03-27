using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Domain.Entities
{
    public class RescueOperation
    {
        public Guid RescueOperationId { get; set; }

        public Guid RescueRequestId { get; set; }

        public Guid? TeamId { get; set; }
        public Guid? ReliefStationId { get; set; }

        public DateTime StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }

        public RescueOperationStatus Status { get; set; } = RescueOperationStatus.Pending;
        public string? Note { get; set; }

        public RescueRequest RescueRequest { get; set; } = default!;
        public Team? Team { get; set; }
        public ReliefStation? ReliefStation { get; set; }
        public ICollection<TeamTrackingPoint> TrackingPoints { get; set; } = new List<TeamTrackingPoint>();
    }
}
