using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ReliefManagementSystem.Domain.Entities;

[Index("CampaignId", Name = "IX_CampaignTasks_CampaignId")]
[Index("CampaignTeamId", Name = "IX_CampaignTasks_CampaignTeamId")]
public partial class CampaignTask
{
    [Key]
    public Guid CampaignTaskId { get; set; }

    public Guid CampaignTeamId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime? DueDate { get; set; }

    public int Status { get; set; }

    public int Priority { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid? CampaignId { get; set; }

    public virtual Campaign? Campaign { get; set; }

    public virtual CampaignTeam CampaignTeam { get; set; } = null!;
    public virtual ICollection<MemberTask> MemberTasks { get; set; } = new List<MemberTask>();
}
