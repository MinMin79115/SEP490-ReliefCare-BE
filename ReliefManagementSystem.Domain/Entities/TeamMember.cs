using Microsoft.EntityFrameworkCore;
using ReliefManagementSystem.Domain.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ReliefManagementSystem.Domain.Entities;

[PrimaryKey("TeamId", "UserId")]
[Index("UserId", Name = "IX_TeamMembers_UserId")]
public partial class TeamMember
{
    [Key]
    public Guid TeamId { get; set; }

    [Key]
    public Guid UserId { get; set; }

    public TeamRole RoleTeam { get; set; }

    public DateTime JoinedAt { get; set; }

    public virtual Team Team { get; set; } = null!;


    public virtual ApplicationUser User { get; set; } = null!;
}
