using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ReliefManagementSystem.Domain.Entities;

[Index("ReliefRequestId", Name = "IX_ReliefNeedItems_ReliefRequestId")]
public partial class ReliefNeedItem
{
    [Key]
    public Guid ReliefNeedItemId { get; set; }

    public Guid ReliefRequestId { get; set; }

    public string NeedType { get; set; } = null!;

    public string UrgencyLevel { get; set; } = null!;

    public int PeopleCount { get; set; }

    public string? Note { get; set; }

    public virtual ReliefRequest ReliefRequest { get; set; } = null!;
}
