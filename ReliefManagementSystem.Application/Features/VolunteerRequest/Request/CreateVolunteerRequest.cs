using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Application.Features.VolunteerRequest.Request
{
   public class CreateVolunteerRequest
    {
        public List<Guid> SkillIds { get; set; }
        public string Descriptions { get; set; } = null!;
        public int? YearsOfExperience { get; set; }
        public List<CreateVolunteerCertificateRequest> Certificates { get; set; } = new();

    }
}
