using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Domain.Entities
{
    public class Request
    {
        public Guid RequestId { get; set; } 
        public RequestType RequestType { get; set; }
        public string Description { get; set; } = string.Empty;

        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double? Accuracy { get; set; }
        public string Address { get; set; } = string.Empty;

        public Guid? ReporterUserId { get; set; } 
        public string ReporterFullName { get; set; } = string.Empty;
        public string ReporterPhone { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public ApplicationUser? ReporterUser { get; set; }
        public ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
        public ICollection<RequestVerification> Verifications { get; set; } = new List<RequestVerification>();
    }
}


