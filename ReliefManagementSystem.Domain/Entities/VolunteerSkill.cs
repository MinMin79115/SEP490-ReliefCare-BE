using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ReliefManagementSystem.Domain.Entities;

[PrimaryKey("VolunteerProfileId", "SkillId")]
[Index("SkillId", Name = "IX_VolunteerSkills_SkillId")]
public partial class VolunteerSkill
{
    [Key]
    public Guid VolunteerProfileId { get; set; }

    [Key]
    public Guid SkillId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Skill Skill { get; set; } = null!;

    public virtual VolunteerProfile VolunteerProfile { get; set; } = null!;
}
