using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Domain.Entities
{
    public class Attachment
    {
        public Guid Id { get; set; }
        public string Type { get; set; } // image | video
        public string Url { get; set; }

        public Guid EmergencyRequestId { get; set; }
    }
}
