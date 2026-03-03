using ReliefManagementSystem.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Domain.Entities
{
    public class ReliefNeedItem
    {
        public Guid  ReliefNeedItemId { get; set; }
        public Guid ReliefRequestId { get; set; }
        public ReliefNeedType NeedType { get; set; }
        public UrgencyLevel UrgencyLevel { get; set; }
        public int PeopleCount { get; set; }
        public string? Note { get; set; }

        public ReliefRequest ReliefRequest { get; set; } = default!;
    }
}
