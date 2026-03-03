using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ReliefManagementSystem.Domain.Entities;

[Index("AssignedLocationId", Name = "IX_ManagerProfiles_AssignedLocationId")]
[Index("UserId", Name = "IX_ManagerProfiles_UserId", IsUnique = true)]
public partial class ManagerProfile
{
    [Key]
    public Guid ManagerProfileId { get; set; }

    public Guid UserId { get; set; }

    public string Level { get; set; } = null!;

    public Guid? AssignedLocationId { get; set; }

    public DateTime AppointedAt { get; set; }

    public string? Notes { get; set; }

    [ForeignKey("AssignedLocationId")]
    [InverseProperty("ManagerProfiles")]
    public virtual Location? AssignedLocation { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("ManagerProfile")]
    public virtual AspNetUser User { get; set; } = null!;
}
