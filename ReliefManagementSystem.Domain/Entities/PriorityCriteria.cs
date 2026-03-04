using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ReliefManagementSystem.Domain.Entities;

[Index("Code", Name = "IX_PriorityCriterias_Code", IsUnique = true)]
public partial class PriorityCriteria
{
    [Key]
    public Guid PriorityCriteriaId { get; set; }

    [StringLength(200)]
    public string Name { get; set; } = null!;

    public int Point { get; set; }

    public string DisasterType { get; set; } = null!;

    [StringLength(50)]
    public string Code { get; set; } = null!;

    public string Description { get; set; } = null!;

    public string Status { get; set; } = null!;

    public virtual ICollection<RescueRequestPriority> RescueRequestPriorities { get; set; } = new List<RescueRequestPriority>();
}
