using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ReliefManagementSystem.Domain.Entities;

[Index("VolunteerProfileId", Name = "IX_VolunteerCertificates_VolunteerProfileId")]
public partial class VolunteerCertificate
{
    [Key]
    public Guid CertificateId { get; set; }

    public Guid VolunteerProfileId { get; set; }

    [StringLength(200)]
    public string Name { get; set; } = null!;

    public string? IssuedBy { get; set; }

    public DateTime? IssuedDate { get; set; }

    public DateTime? ExpiryDate { get; set; }

    public string? FileUrl { get; set; }

    [ForeignKey("VolunteerProfileId")]
    [InverseProperty("VolunteerCertificates")]
    public virtual VolunteerProfile VolunteerProfile { get; set; } = null!;
}
