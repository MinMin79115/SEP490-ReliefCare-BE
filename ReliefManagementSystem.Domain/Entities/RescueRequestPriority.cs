using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ReliefManagementSystem.Domain.Entities;


public partial class RescueRequestPriority
{
    public Guid RescueRequestId { get; set; }

    [Key]
    public Guid PriorityCriteriaId { get; set; }

    public int AppliedPoint { get; set; }

    public string Status { get; set; } = null!;


    public virtual PriorityCriteria PriorityCriteria { get; set; } = null!;

    public virtual RescueRequest RescueRequest { get; set; } = null!;
}
