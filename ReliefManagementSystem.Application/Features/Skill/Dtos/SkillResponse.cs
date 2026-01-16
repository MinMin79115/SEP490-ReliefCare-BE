using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Application.Features.Skill.Dtos
{
    public class SkillResponse
    {
        public Guid SkillId { get; set; }
        public string Code { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string Description { get; set; } = default!;
    }
}
