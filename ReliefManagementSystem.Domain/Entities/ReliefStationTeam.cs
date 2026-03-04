using Microsoft.EntityFrameworkCore;
using ReliefManagementSystem.Domain.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ReliefManagementSystem.Domain.Entities;

[Index("ReliefStationId", "TeamId", Name = "IX_ReliefStationTeams_ReliefStationId_TeamId", IsUnique = true)]
[Index("TeamId", Name = "IX_ReliefStationTeams_TeamId")]
public partial class ReliefStationTeam
{
    [Key]
    public Guid ReliefStationTeamId { get; set; }

    public Guid ReliefStationId { get; set; }

    public Guid TeamId { get; set; }

    public bool IsActive { get; set; }

    public ReliefTeamAssignmentStatus Status { get; set; }

    public virtual ReliefStation ReliefStation { get; set; } = null!;
    public virtual Team Team { get; set; } = null!;
}
