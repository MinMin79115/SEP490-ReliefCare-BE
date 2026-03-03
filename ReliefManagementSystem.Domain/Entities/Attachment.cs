using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ReliefManagementSystem.Domain.Entities;

[Index("RequestId", Name = "IX_Attachments_RequestId")]
public partial class Attachment
{
    [Key]
    public Guid AttachmentId { get; set; }

    public Guid RequestId { get; set; }

    public string? Url { get; set; }

    public string? FileType { get; set; }

    public DateTime UploadedAt { get; set; }

    [ForeignKey("RequestId")]
    [InverseProperty("Attachments")]
    public virtual Request Request { get; set; } = null!;
}
