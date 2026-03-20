using ReliefManagementSystem.Domain.Common;
using ReliefManagementSystem.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Domain.Entities
{
    public class RescueRequest : Request
    {
        public DisasterType DisasterType { get; set; }
        public RescueRequestType RescueRequestType { get; set; }
        public int? PriorityPoint { get; set; }
        public RescuePriorityLevel RescuePriorityLevel { get; set; }
        public string? Note { get; set; }
        public RescueRequestStatus RescueRequestStatus { get; set; }
        public DispatchMode DispatchMode { get; set; }
        public ICollection<RescueRequestPriority> RescueRequestPriorities { get; set; } = new List<RescueRequestPriority>();
        public ICollection<RescueOperation> RescueOperations { get; set; } = new List<RescueOperation>();
    }
}
