using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Domain.Entities
{
    public class Attachment
    {
        public Guid AttachmentId { get; set; }

        public Guid RequestId { get; set; }

        public string? Url { get; set; }
        public string? FileType { get; set; }
        public DateTime UploadedAt { get; set; }

        public Request? Request { get; set; } 
    }
}
