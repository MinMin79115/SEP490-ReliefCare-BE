using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Application.Features.VolunteerRequest.Request
{
    public class AddVolunteerRequest
    {
        public List<Guid> SkillIds { get; set; } = new();
    }
}
