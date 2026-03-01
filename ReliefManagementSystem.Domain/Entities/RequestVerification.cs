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
        public Guid Id { get; private set; }
        public Guid EmergencyRequestId { get; private set; }
        public VerificationMethod Method { get; private set; }
        public VerificationResult Result { get; private set; }
        public Guid VerifiedBy { get; private set; } // UserId của operator
        public DateTime VerifiedAt { get; private set; }
        public string? Note { get; private set; }

        protected RequestVerification() { } // EF Core

        private RequestVerification(
            Guid requestId,
            VerificationMethod method,
            VerificationResult result,
            Guid verifiedBy,
            string? note)
        {
            Id = Guid.NewGuid();
            EmergencyRequestId = requestId;
            Method = method;
            Result = result;
            VerifiedBy = verifiedBy;
            VerifiedAt = DateTime.UtcNow;
            Note = note?.Trim();
        }

        internal static RequestVerification Create(
            Guid requestId,
            VerificationMethod method,
            VerificationResult result,
            Guid verifiedBy,
            string? note = null)
            => new(requestId, method, result, verifiedBy, note);
    }
}
