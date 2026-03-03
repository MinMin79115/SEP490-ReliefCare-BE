using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ReliefManagementSystem.Domain.Entities;

[Index("RequestId", Name = "IX_RequestVerifications_RequestId")]
[Index("VerifiedBy", Name = "IX_RequestVerifications_VerifiedBy")]
public partial class RequestVerification
{
    [Key]
    public Guid RequestVerificationId { get; set; }

    public Guid RequestId { get; set; }

    public string Method { get; set; } = null!;

    public string Result { get; set; } = null!;

    public Guid VerifiedBy { get; set; }

    public DateTime VerifiedAt { get; set; }

    public string Note { get; set; } = null!;

    [ForeignKey("RequestId")]
    [InverseProperty("RequestVerifications")]
    public virtual Request Request { get; set; } = null!;

    [ForeignKey("VerifiedBy")]
    [InverseProperty("RequestVerifications")]
    public virtual AspNetUser VerifiedByNavigation { get; set; } = null!;
}
