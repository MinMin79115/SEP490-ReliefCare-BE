using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Domain.Entities
{
    public class VolunteerSkill
    {
        public int VolunteerProfileId { get; set; }
        public VolunteerProfile VolunteerProfile { get; set; }

        public int SkillId { get; set; }
        public Skill Skill { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    }
}
