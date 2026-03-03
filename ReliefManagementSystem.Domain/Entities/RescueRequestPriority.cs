using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ReliefManagementSystem.Domain.Entities;

[PrimaryKey("RescueRequestId", "PriorityCriteriaId")]
[Index("PriorityCriteriaId", Name = "IX_RescueRequestPriorities_PriorityCriteriaId")]
public partial class RescueRequestPriority
{
    [Key]
    public Guid RescueRequestId { get; set; }

    [Key]
    public Guid PriorityCriteriaId { get; set; }

    public int AppliedPoint { get; set; }

    public string Status { get; set; } = null!;

    [ForeignKey("PriorityCriteriaId")]
    [InverseProperty("RescueRequestPriorities")]
    public virtual PriorityCriteria PriorityCriteria { get; set; } = null!;

    [ForeignKey("RescueRequestId")]
    [InverseProperty("RescueRequestPriorities")]
    public virtual RescueRequest RescueRequest { get; set; } = null!;
}
