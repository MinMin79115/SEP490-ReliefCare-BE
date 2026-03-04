using Microsoft.EntityFrameworkCore;
using ReliefManagementSystem.Domain.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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

    public LocationLevel Level { get; set; }

    public int Status { get; set; }

    public string Path { get; set; } = null!;

    public virtual ICollection<Campaign> Campaigns { get; set; } = new List<Campaign>();

    public virtual ICollection<Location> InverseParent { get; set; } = new List<Location>();

    public virtual ICollection<ManagerProfile> ManagerProfiles { get; set; } = new List<ManagerProfile>();

    public virtual Location? Parent { get; set; }

    public virtual ICollection<ReliefStation> ReliefStations { get; set; } = new List<ReliefStation>();
}
