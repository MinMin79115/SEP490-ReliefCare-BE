using Microsoft.EntityFrameworkCore;
using ReliefManagementSystem.Domain.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ReliefManagementSystem.Domain.Entities;

[Index("LeaderId", Name = "IX_Teams_LeaderId")]
[Index("ModeratorId", Name = "IX_Teams_ModeratorId")]
public partial class Team
{
    [Key]
    public Guid TeamId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public Guid ModeratorId { get; set; }

    public Guid? LeaderId { get; set; }
    public TeamStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<CampaignTeam> CampaignTeams { get; set; } = new List<CampaignTeam>();
    public virtual ApplicationUser? Leader { get; set; }

    public virtual ApplicationUser Moderator { get; set; } = null!;


    public virtual ICollection<ReliefStationTeam> ReliefStationTeams { get; set; } = new List<ReliefStationTeam>();

    public virtual ICollection<RescueOperation> RescueOperations { get; set; } = new List<RescueOperation>();

    public virtual ICollection<TeamJoinRequest> TeamJoinRequests { get; set; } = new List<TeamJoinRequest>();


    public virtual ICollection<TeamMember> TeamMembers { get; set; } = new List<TeamMember>();
}
