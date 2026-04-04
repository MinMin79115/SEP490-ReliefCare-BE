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

        public Guid? AssignedReliefStationId { get; set; }

        public DateTime? ApprovedAt { get; set; }

        public DateTime? CompletedAt { get; set; }

        public ReliefRequestStatus Status { get; set; }

        public Campaign? Campaign { get; set; }

        public ReliefStation? AssignedReliefStation { get; set; }

        public ICollection<ReliefNeedItem> ReliefNeedItems { get; set; } = new List<ReliefNeedItem>();

        public ICollection<DistributionSessionRequest> DistributionSessionRequests { get; set; } = new List<DistributionSessionRequest>();

        public ICollection<ReliefFulfillment> ReliefFulfillments { get; set; } = new List<ReliefFulfillment>();

    }
}
    
