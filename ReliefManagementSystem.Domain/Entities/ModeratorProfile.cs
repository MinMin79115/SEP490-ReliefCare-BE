using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ReliefManagementSystem.Domain.Entities;

[Index("UserId", Name = "IX_ModeratorProfiles_UserId", IsUnique = true)]
public partial class ModeratorProfile
{
    [Key]
    public Guid ModeratorProfileId { get; set; }

    public Guid UserId { get; set; }

    public string? AssignedArea { get; set; }

    public DateTime AppointedAt { get; set; }

    public string? Notes { get; set; }

    public virtual ApplicationUser User { get; set; } = null!;
}
