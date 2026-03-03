using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ReliefManagementSystem.Domain.Entities;

[PrimaryKey("TeamId", "UserId")]
[Index("UserId", Name = "IX_TeamMembers_UserId")]
public partial class TeamMember
{
    [Key]
    public Guid TeamId { get; set; }

    [Key]
    public Guid UserId { get; set; }

    public int RoleTeam { get; set; }

    public DateTime JoinedAt { get; set; }

    [ForeignKey("TeamId")]
    [InverseProperty("TeamMembers")]
    public virtual Team Team { get; set; } = null!;

    [ForeignKey("UserId")]
    [InverseProperty("TeamMembers")]
    public virtual AspNetUser User { get; set; } = null!;
}
