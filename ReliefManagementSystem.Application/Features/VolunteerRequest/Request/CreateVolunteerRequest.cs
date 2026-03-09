using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Application.Features.VolunteerRequest.Request
{
   public class CreateVolunteerRequest
    {
        public string Descriptions { get; set; } = null!;
        public List<Guid> SkillIds { get; set; }
        public int? YearsOfExperience { get; set; }
    }
}
