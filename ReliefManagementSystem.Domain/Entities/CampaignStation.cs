using ReliefManagementSystem.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Domain.Entities
{
    public class CampaignStation
    {
        public Guid CampaignId { get; set; }
        public Campaign Campaign { get; set; } = null!;

        public Guid ReliefStationId { get; set; }
        public ReliefStation ReliefStation { get; set; } = null!;

        public bool IsActive { get; set; } = true;
        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    }
}
