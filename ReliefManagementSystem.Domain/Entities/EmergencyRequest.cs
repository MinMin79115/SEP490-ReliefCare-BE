using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Domain.Entities
{
    public class EmergencyRequest
    {
        public Guid Id { get; set; }

        public NeedType NeedType { get; set; }
        public DisasterType? DisasterType { get; set; } // Only rescue

        public string Description { get; set; }

        public int? PeopleCount { get; set; }
        public GroupSizeLevel? GroupSizeLevel { get; set; }
        public UrgencyLevel? UrgencyLevel { get; set; } // Relief

        public GeoLocation Location { get; set; }
        public string Address { get; set; }

        // Rescue only
        public PriorityCriteria PriorityCriteria { get; set; }

        // User info
        public Guid? UserId { get; set; } 

        // Anonymous SOS
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string AvatarUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
    }

}
