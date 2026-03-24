using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Application.Features.VolunteerRequest.Request
{
    public class CreateVolunteerCertificateRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? IssuedBy { get; set; }
        public DateOnly? IssuedDate { get; set; }
        public DateOnly? ExpiryDate { get; set; }
        public string? FileUrl { get; set; }
    }
}
