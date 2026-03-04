using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ReliefManagementSystem.Domain.Entities;

[Index("CampaignId", "TeamId", Name = "IX_CampaignTeams_CampaignId_TeamId", IsUnique = true)]
[Index("TeamId", Name = "IX_CampaignTeams_TeamId")]
public partial class CampaignTeam
{
    [Key]
    public Guid CampaignTeamId { get; set; }

    public Guid CampaignId { get; set; }

    public Guid TeamId { get; set; }

    public int Role { get; set; }

    public int Status { get; set; }

    public DateTime AssignedAt { get; set; }

    public bool IsDelete { get; set; }

    public virtual Campaign Campaign { get; set; } = null!;

    public virtual ICollection<CampaignTask> CampaignTasks { get; set; } = new List<CampaignTask>();

    public virtual Team Team { get; set; } = null!;
}
