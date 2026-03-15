using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Application.Features.VolunteerRequest.Response
{
    public class VolunteerCertificateResponse
    {
            public string Name { get; set; } = string.Empty;
            public string? IssuedBy { get; set; }
            public DateTime? IssuedDate { get; set; }
            public DateTime? ExpiryDate { get; set; }
            public string? FileUrl { get; set; }
    }
}
