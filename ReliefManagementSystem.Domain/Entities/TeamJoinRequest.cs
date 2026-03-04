using Microsoft.EntityFrameworkCore;
using ReliefManagementSystem.Domain.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ReliefManagementSystem.Domain.Entities;

[Index("ReviewedBy", Name = "IX_TeamJoinRequests_ReviewedBy")]
[Index("TeamId", Name = "IX_TeamJoinRequests_TeamId")]
[Index("VolunteerId", Name = "IX_TeamJoinRequests_VolunteerId")]
public partial class TeamJoinRequest
{
    [Key]
    public Guid Id { get; set; }

    public Guid TeamId { get; set; }

    public Guid VolunteerId { get; set; }

    public TeamJoinRequestStatus Status { get; set; } = TeamJoinRequestStatus.Pending;

    public string? Reason { get; set; }

    public DateTime? RejectedAt { get; set; }

    public Guid? RejectedBy { get; set; }

    public DateTime? ApprovedAt { get; set; }

    public Guid? ApprovedBy { get; set; }

    public string? ReviewNote { get; set; }

    public DateTime? ReviewedAt { get; set; }

    public Guid? ReviewedBy { get; set; }

    public DateTime CreatedAt { get; set; }


    public virtual ApplicationUser? ReviewedByNavigation { get; set; }

    public virtual Team Team { get; set; } = null!;
    public virtual ApplicationUser Volunteer { get; set; } = null!;
}
