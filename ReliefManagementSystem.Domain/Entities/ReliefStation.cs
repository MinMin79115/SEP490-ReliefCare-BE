using Microsoft.EntityFrameworkCore;
using ReliefManagementSystem.Domain.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ReliefManagementSystem.Domain.Entities;

[Index("LocationId", Name = "IX_ReliefStations_LocationId")]
[Index("ManagerId", Name = "IX_ReliefStations_ManagerId")]
[Index("ParentReliefStationId", Name = "IX_ReliefStations_ParentReliefStationId")]
public partial class ReliefStation
{
    [Key]
    public Guid ReliefStationId { get; set; }

    [StringLength(255)]
    public string Name { get; set; } = null!;

    public Guid? ManagerId { get; set; }

    public Guid LocationId { get; set; }

    public string? Address { get; set; }

    public string? ContactNumber { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public Guid CreatedBy { get; set; }

    public double Longitude { get; set; }

    public double Latitude { get; set; }

    public Guid? ParentReliefStationId { get; set; }

    public ReliefStationStatus Status { get; set; }

    public ReliefStationLevel Level { get; set; }

    public virtual ICollection<ApplicationUser> AspNetUsers { get; set; } = new List<ApplicationUser>();

    public virtual ICollection<Campaign> Campaigns { get; set; } = new List<Campaign>();

    public virtual ICollection<Inventory> Inventories { get; set; } = new List<Inventory>();

    public virtual ICollection<ReliefStation> InverseParentReliefStation { get; set; } = new List<ReliefStation>();
    public virtual Location Location { get; set; } = null!;

    public virtual ApplicationUser? Manager { get; set; }
    public virtual ReliefStation? ParentReliefStation { get; set; }

    public virtual ICollection<ReliefStationTeam> ReliefStationTeams { get; set; } = new List<ReliefStationTeam>();

    public virtual ICollection<RescueOperation> RescueOperations { get; set; } = new List<RescueOperation>();

    public virtual ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
}
