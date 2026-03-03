using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ReliefManagementSystem.Domain.Entities;

[Index("ParentId", Name = "IX_Locations_ParentId")]
public partial class Location
{
    [Key]
    public Guid LocationId { get; set; }

    public Guid? ParentId { get; set; }

    public string Name { get; set; } = null!;

    public decimal PopulationDensity { get; set; }

    public decimal Area { get; set; }

    public long Population { get; set; }

    public string NormalizedName { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public int Level { get; set; }

    public int Status { get; set; }

    public string Path { get; set; } = null!;

    [InverseProperty("Location")]
    public virtual ICollection<Campaign> Campaigns { get; set; } = new List<Campaign>();

    [InverseProperty("Parent")]
    public virtual ICollection<Location> InverseParent { get; set; } = new List<Location>();

    [InverseProperty("AssignedLocation")]
    public virtual ICollection<ManagerProfile> ManagerProfiles { get; set; } = new List<ManagerProfile>();

    [ForeignKey("ParentId")]
    [InverseProperty("InverseParent")]
    public virtual Location? Parent { get; set; }

    [InverseProperty("Location")]
    public virtual ICollection<ReliefStation> ReliefStations { get; set; } = new List<ReliefStation>();
}
