using ReliefManagementSystem.Domain.Enum;
using ReliefManagementSystem.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Domain.Entities
{
    public class ReliefRequest : Request
    {
        public Guid? CampaignId { get; set; }

        public ReliefRequestStatus Status { get; set; }

        public Campaign? Campaign { get; set; } 

        public ICollection<ReliefNeedItem> ReliefNeedItems { get; set; } = new List<ReliefNeedItem>();

    }
}
    