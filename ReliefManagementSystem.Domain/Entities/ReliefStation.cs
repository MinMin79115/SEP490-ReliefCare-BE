using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

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

    public int Status { get; set; }

    public int Level { get; set; }

    [InverseProperty("ManagedStationReliefStation")]
    public virtual ICollection<AspNetUser> AspNetUsers { get; set; } = new List<AspNetUser>();

    [InverseProperty("CreatedByStation")]
    public virtual ICollection<Campaign> Campaigns { get; set; } = new List<Campaign>();

    [InverseProperty("ReliefStation")]
    public virtual ICollection<Inventory> Inventories { get; set; } = new List<Inventory>();

    [InverseProperty("ParentReliefStation")]
    public virtual ICollection<ReliefStation> InverseParentReliefStation { get; set; } = new List<ReliefStation>();

    [ForeignKey("LocationId")]
    [InverseProperty("ReliefStations")]
    public virtual Location Location { get; set; } = null!;

    [ForeignKey("ManagerId")]
    [InverseProperty("ReliefStations")]
    public virtual AspNetUser? Manager { get; set; }

    [ForeignKey("ParentReliefStationId")]
    [InverseProperty("InverseParentReliefStation")]
    public virtual ReliefStation? ParentReliefStation { get; set; }

    [InverseProperty("ReliefStation")]
    public virtual ICollection<ReliefStationTeam> ReliefStationTeams { get; set; } = new List<ReliefStationTeam>();

    [InverseProperty("ReliefStation")]
    public virtual ICollection<RescueOperation> RescueOperations { get; set; } = new List<RescueOperation>();

    [InverseProperty("ReliefStation")]
    public virtual ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
}
