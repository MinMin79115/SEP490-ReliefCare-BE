using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ReliefManagementSystem.Domain.Entities;

[Index("CampaignTaskId", Name = "IX_MemberTasks_CampaignTaskId")]
[Index("VolunteerProfileId", Name = "IX_MemberTasks_VolunteerProfileId")]
public partial class MemberTask
{
    [Key]
    public Guid MemberTaskId { get; set; }

    public Guid CampaignTaskId { get; set; }

    public Guid VolunteerProfileId { get; set; }

    public string SubTaskTitle { get; set; } = null!;

    public string? TaskNote { get; set; }

    public DateTime? AssignedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public int Status { get; set; }

    [ForeignKey("CampaignTaskId")]
    [InverseProperty("MemberTasks")]
    public virtual CampaignTask CampaignTask { get; set; } = null!;

    [ForeignKey("VolunteerProfileId")]
    [InverseProperty("MemberTasks")]
    public virtual VolunteerProfile VolunteerProfile { get; set; } = null!;
}
