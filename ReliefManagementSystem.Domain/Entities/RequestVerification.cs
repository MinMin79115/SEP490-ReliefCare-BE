using ReliefManagementSystem.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Domain.Entities
{
    public class RequestVerification
    {
        public Guid RequestVerificationId { get; set; }

        public Guid RequestId { get; set; }

        public VerificationMethod Method { get; set; }
        public string? Note { get; set; }
        public string? Reason { get; set; }
        public RequestVerificationStatus Status { get; set; }
        public Guid? VerifiedBy { get; set; }
        public DateTime? VerifiedAt { get; set; }

        public Request? Request { get; set; }
        public ApplicationUser? Verifier { get; set; }

    }
}
