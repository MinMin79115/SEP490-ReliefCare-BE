using Microsoft.EntityFrameworkCore;
using ReliefManagementSystem.Domain.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ReliefManagementSystem.Domain.Entities;

public partial class RescueRequest
{
    [Key]
    public Guid RequestId { get; set; }

    public string DisasterType { get; set; } = null!;

    public int? Priority { get; set; }

    public string? Note { get; set; }

    public RescueRequestStatus RescueRequestStatus { get; set; }


    public virtual Request Request { get; set; } = null!;

    public virtual ICollection<RescueOperation> RescueOperations { get; set; } = new List<RescueOperation>();

    public virtual ICollection<RescueRequestPriority> RescueRequestPriorities { get; set; } = new List<RescueRequestPriority>();
}
