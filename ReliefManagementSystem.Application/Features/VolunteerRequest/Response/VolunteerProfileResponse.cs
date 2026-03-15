using ReliefManagementSystem.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReliefManagementSystem.Domain.Entities;

namespace ReliefManagementSystem.Application.Features.VolunteerRequest.Response
{
    public class VolunteerProfileResponse
    {
        public Guid VolunteerProfileId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string? Descriptions { get; set; }
        public VerificationStatus VerificationStatus { get; set; }
        public int? YearsOfExperience { get; set; }
        public List<Guid> Skills { get; set; } = new List<Guid>();
       public List<VolunteerCertificateResponse> Certificates { get; set; } = new List<VolunteerCertificateResponse>();
    }
}
