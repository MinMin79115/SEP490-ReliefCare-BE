using ReliefManagementSystem.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Domain.Entities
{
    public class CampaignVehicle
    {
        public Guid CampaignVehicleId { get; set; }

        public Guid VehicleId { get; set; }
        public Guid CampaignId { get; set; }

        public Guid? AssignedDriverId { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public VehicleAssignmentStatus Status { get; set; }

        public string? Note { get; set; } 

        public virtual Vehicle Vehicle { get; set; } = default!;
        public virtual Campaign Campaign { get; set; } = default!;
        public virtual VolunteerProfile? Driver { get; set; }
    }
}
