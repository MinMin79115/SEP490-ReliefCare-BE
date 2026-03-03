using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ReliefManagementSystem.Domain.Entities;

[Index("ReliefStationId", Name = "IX_RescueOperations_ReliefStationId")]
[Index("RescueRequestId", Name = "IX_RescueOperations_RescueRequestId")]
[Index("TeamId", Name = "IX_RescueOperations_TeamId")]
public partial class RescueOperation
{
    [Key]
    public Guid RescueOperationId { get; set; }

    public Guid RescueRequestId { get; set; }

    public Guid? TeamId { get; set; }

    public Guid? ReliefStationId { get; set; }

    public DateTime StartedAt { get; set; }

    public DateTime? EndedAt { get; set; }

    public string Status { get; set; } = null!;

    public string Note { get; set; } = null!;

    [ForeignKey("ReliefStationId")]
    [InverseProperty("RescueOperations")]
    public virtual ReliefStation? ReliefStation { get; set; }

    [ForeignKey("RescueRequestId")]
    [InverseProperty("RescueOperations")]
    public virtual RescueRequest RescueRequest { get; set; } = null!;

    [ForeignKey("TeamId")]
    [InverseProperty("RescueOperations")]
    public virtual Team? Team { get; set; }
}
