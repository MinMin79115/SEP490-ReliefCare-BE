using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ReliefManagementSystem.Domain.Entities;

[Index("CampaignId", Name = "IX_ReliefRequests_CampaignId")]
public partial class ReliefRequest
{
    [Key]
    public Guid RequestId { get; set; }

    public Guid? CampaignId { get; set; }

    public string Status { get; set; } = null!;

    [ForeignKey("CampaignId")]
    [InverseProperty("ReliefRequests")]
    public virtual Campaign? Campaign { get; set; }

    [InverseProperty("ReliefRequest")]
    public virtual ICollection<ReliefNeedItem> ReliefNeedItems { get; set; } = new List<ReliefNeedItem>();

    [ForeignKey("RequestId")]
    [InverseProperty("ReliefRequest")]
    public virtual Request Request { get; set; } = null!;
}
