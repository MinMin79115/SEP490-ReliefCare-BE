using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

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

    public int Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    [InverseProperty("Team")]
    public virtual ICollection<CampaignTeam> CampaignTeams { get; set; } = new List<CampaignTeam>();

    [ForeignKey("LeaderId")]
    [InverseProperty("TeamLeaders")]
    public virtual AspNetUser? Leader { get; set; }

    [ForeignKey("ModeratorId")]
    [InverseProperty("TeamModerators")]
    public virtual AspNetUser Moderator { get; set; } = null!;

    [InverseProperty("Team")]
    public virtual ICollection<ReliefStationTeam> ReliefStationTeams { get; set; } = new List<ReliefStationTeam>();

    [InverseProperty("Team")]
    public virtual ICollection<RescueOperation> RescueOperations { get; set; } = new List<RescueOperation>();

    [InverseProperty("Team")]
    public virtual ICollection<TeamJoinRequest> TeamJoinRequests { get; set; } = new List<TeamJoinRequest>();

    [InverseProperty("Team")]
    public virtual ICollection<TeamMember> TeamMembers { get; set; } = new List<TeamMember>();
}
