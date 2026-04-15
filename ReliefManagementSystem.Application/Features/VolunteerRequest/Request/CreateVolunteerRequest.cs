using ReliefManagementSystem.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Application.Features.VolunteerRequest.Request
{
    public class CreateVolunteerRequest
    {
        public Guid? CampaignId { get; set; }
        public List<Guid> SkillIds { get; set; }
        public string Descriptions { get; set; } = null!;
        public int? YearsOfExperience { get; set; }
        public TeamRolePreference PreferredTeamRole { get; set; } = TeamRolePreference.Member;
        public List<CreateVolunteerCertificateRequest> Certificates { get; set; } = new();

    }
}
