using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ReliefManagementSystem.Domain.Entities;

public partial class RescueRequest
{
    [Key]
    public Guid RequestId { get; set; }

    public string DisasterType { get; set; } = null!;

    public int? Priority { get; set; }

    public string? Note { get; set; }

    public string RescueRequestStatus { get; set; } = null!;

    [ForeignKey("RequestId")]
    [InverseProperty("RescueRequest")]
    public virtual Request Request { get; set; } = null!;

    [InverseProperty("RescueRequest")]
    public virtual ICollection<RescueOperation> RescueOperations { get; set; } = new List<RescueOperation>();

    [InverseProperty("RescueRequest")]
    public virtual ICollection<RescueRequestPriority> RescueRequestPriorities { get; set; } = new List<RescueRequestPriority>();
}
