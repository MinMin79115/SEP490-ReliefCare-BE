using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Domain.Entities
{
    public class AuditLog
    {
        public Guid AuditLogId { get; set; }
        public string EntityName { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public Guid? UserId { get; set; }

        public string? OldValues { get; set; }
        public string? NewValues { get; set; }
        public string? PrimaryKey { get; set; }
    }
}
