using Microsoft.EntityFrameworkCore;
using ReliefManagementSystem.Domain.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ReliefManagementSystem.Domain.Entities;

[Index("AssignedLocationId", Name = "IX_ManagerProfiles_AssignedLocationId")]
[Index("UserId", Name = "IX_ManagerProfiles_UserId", IsUnique = true)]
public partial class ManagerProfile
{
    [Key]
    public Guid ManagerProfileId { get; set; }

    public Guid UserId { get; set; }

    public ReliefStationLevel Level { get; set; }

    public Guid? AssignedLocationId { get; set; }

    public DateTime AppointedAt { get; set; }

    public string? Notes { get; set; }
    public virtual Location? AssignedLocation { get; set; }

    public virtual ApplicationUser User { get; set; } = null!;
}
