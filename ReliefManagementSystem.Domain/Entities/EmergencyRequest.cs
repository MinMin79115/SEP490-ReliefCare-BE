using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Domain.Entities
{
    public class EmergencyRequest
    {
        public Guid EmergencyRequestId { get; set; }
        //public RequestType RequestType { get; set; }
        public RequestStatus Status { get; set; } = RequestStatus.Draft;

        public string Description { get; set; } = string.Empty;

        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double? Accuracy { get; set; }
        public string Address { get; set; } = string.Empty;

        public Guid? ReporterUserId { get; set; } 
        public string ReporterFullName { get; set; } = string.Empty;
        public string ReporterPhone { get; set; } = string.Empty;

        public Guid? VerifiedBy { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public VerificationMethod? VerificationMethod { get; set; }
        public string? VerificationNote { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public ApplicationUser? ReporterUser { get; set; }
        public ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
        public ICollection<RequestVerification> Verifications { get; set; } = new List<RequestVerification>();

        public void Submit()
        {
            if (Status != RequestStatus.Draft)
                throw new InvalidOperationException("Only draft requests can be submitted.");

            Status = RequestStatus.Submitted;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Verify(Guid verifiedBy, VerificationMethod method,
                           VerificationResult result, string? note = null)
        {
            if (Status != RequestStatus.Submitted)
                throw new InvalidOperationException("Only submitted requests can be verified.");

            if (result == VerificationResult.Confirmed)
            {
                Status = RequestStatus.Verified;
                VerifiedBy = verifiedBy;
                VerifiedAt = DateTime.UtcNow;
                VerificationMethod = method;
                VerificationNote = note;
            }
            else if (result == VerificationResult.Failed)
            {
                Status = RequestStatus.Rejected;
            }

            UpdatedAt = DateTime.UtcNow;
        }

        public void MarkInProgress()
        {
            if (Status != RequestStatus.Verified)
                throw new InvalidOperationException("Request must be verified before marking in progress.");

            Status = RequestStatus.InProgress;
            UpdatedAt = DateTime.UtcNow;
        }

        public void MarkResolved()
        {
            if (Status != RequestStatus.InProgress)
                throw new InvalidOperationException("Only in-progress requests can be resolved.");

            Status = RequestStatus.Resolved;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Cancel()
        {
            if (Status == RequestStatus.Resolved || Status == RequestStatus.Cancelled)
                throw new InvalidOperationException("Cannot cancel a closed request.");

            Status = RequestStatus.Cancelled;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Reject(string? note = null)
        {
            if (Status != RequestStatus.Submitted)
                throw new InvalidOperationException("Only submitted requests can be rejected.");

            Status = RequestStatus.Rejected;
            VerificationNote = note;
            UpdatedAt = DateTime.UtcNow;
        }

    }

}
