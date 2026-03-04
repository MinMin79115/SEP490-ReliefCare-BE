using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ReliefManagementSystem.Domain.Entities;

[Index("ReporterUserId", Name = "IX_Requests_ReporterUserId")]
public partial class Request
{
    [Key]
    public Guid RequestId { get; set; }

    public int RequestType { get; set; }

    [StringLength(500)]
    public string Description { get; set; } = null!;

    public double Latitude { get; set; }

    public double Longitude { get; set; }

    public double? Accuracy { get; set; }

    public string Address { get; set; } = null!;

    public Guid? ReporterUserId { get; set; }

    public string ReporterFullName { get; set; } = null!;

    public string ReporterPhone { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();

    public virtual ReliefRequest? ReliefRequest { get; set; }

    public virtual ApplicationUser? ReporterUser { get; set; }

    public virtual ICollection<RequestVerification> RequestVerifications { get; set; } = new List<RequestVerification>();

    public virtual RescueRequest? RescueRequest { get; set; }
}
