using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ReliefManagementSystem.Domain.Entities;

[Index("NormalizedEmail", Name = "EmailIndex")]
[Index("ManagedStationReliefStationId", Name = "IX_AspNetUsers_ManagedStationReliefStationId")]
[Index("NormalizedUserName", Name = "UserNameIndex", IsUnique = true)]
public partial class AspNetUser
{
    [Key]
    public Guid Id { get; set; }

    public string? PictureUrl { get; set; }

    public DateTime? DateOfBirth { get; set; }

    public string? Gender { get; set; }

    public string? DisplayName { get; set; }

    [StringLength(256)]
    public string? UserName { get; set; }

    [StringLength(256)]
    public string? NormalizedUserName { get; set; }

    [StringLength(256)]
    public string? Email { get; set; }

    [StringLength(256)]
    public string? NormalizedEmail { get; set; }

    public bool EmailConfirmed { get; set; }

    public string? PasswordHash { get; set; }

    public string? SecurityStamp { get; set; }

    public string? ConcurrencyStamp { get; set; }

    public string? PhoneNumber { get; set; }

    public bool PhoneNumberConfirmed { get; set; }

    public bool TwoFactorEnabled { get; set; }

    public DateTime? LockoutEnd { get; set; }

    public bool LockoutEnabled { get; set; }

    public int AccessFailedCount { get; set; }

    public string? PicturePublicId { get; set; }

    public string? Address { get; set; }

    public Guid? ManagedStationReliefStationId { get; set; }

    [InverseProperty("User")]
    public virtual ICollection<AspNetUserClaim> AspNetUserClaims { get; set; } = new List<AspNetUserClaim>();

    [InverseProperty("User")]
    public virtual ICollection<AspNetUserLogin> AspNetUserLogins { get; set; } = new List<AspNetUserLogin>();

    [InverseProperty("User")]
    public virtual ICollection<AspNetUserToken> AspNetUserTokens { get; set; } = new List<AspNetUserToken>();

    [InverseProperty("CreatedByNavigation")]
    public virtual ICollection<Campaign> Campaigns { get; set; } = new List<Campaign>();

    [InverseProperty("DonorUser")]
    public virtual ICollection<Donation> Donations { get; set; } = new List<Donation>();

    [InverseProperty("CreatedByNavigation")]
    public virtual ICollection<InventoryTransaction> InventoryTransactions { get; set; } = new List<InventoryTransaction>();

    [ForeignKey("ManagedStationReliefStationId")]
    [InverseProperty("AspNetUsers")]
    public virtual ReliefStation? ManagedStationReliefStation { get; set; }

    [InverseProperty("User")]
    public virtual ManagerProfile? ManagerProfile { get; set; }

    [InverseProperty("User")]
    public virtual ModeratorProfile? ModeratorProfile { get; set; }

    [InverseProperty("User")]
    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

    [InverseProperty("Manager")]
    public virtual ICollection<ReliefStation> ReliefStations { get; set; } = new List<ReliefStation>();

    [InverseProperty("VerifiedByNavigation")]
    public virtual ICollection<RequestVerification> RequestVerifications { get; set; } = new List<RequestVerification>();

    [InverseProperty("ReporterUser")]
    public virtual ICollection<Request> Requests { get; set; } = new List<Request>();

    [InverseProperty("ReviewedByNavigation")]
    public virtual ICollection<TeamJoinRequest> TeamJoinRequestReviewedByNavigations { get; set; } = new List<TeamJoinRequest>();

    [InverseProperty("Volunteer")]
    public virtual ICollection<TeamJoinRequest> TeamJoinRequestVolunteers { get; set; } = new List<TeamJoinRequest>();

    [InverseProperty("Leader")]
    public virtual ICollection<Team> TeamLeaders { get; set; } = new List<Team>();

    [InverseProperty("User")]
    public virtual ICollection<TeamMember> TeamMembers { get; set; } = new List<TeamMember>();

    [InverseProperty("Moderator")]
    public virtual ICollection<Team> TeamModerators { get; set; } = new List<Team>();

    [InverseProperty("CreatedByNavigation")]
    public virtual ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();

    [InverseProperty("User")]
    public virtual VolunteerProfile? VolunteerProfile { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("Users")]
    public virtual ICollection<AspNetRole> Roles { get; set; } = new List<AspNetRole>();
}
