using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ReliefManagementSystem.Domain.Entities;

[Index("CampaignId", Name = "IX_Donations_CampaignId")]
[Index("DonorUserId", Name = "IX_Donations_DonorUserId")]
public partial class Donation
{
    [Key]
    public Guid DonationId { get; set; }

    public Guid CampaignId { get; set; }

    public Guid? DonorUserId { get; set; }

    public bool IsAnonymous { get; set; }

    public string? DonorName { get; set; }

    [Precision(18, 2)]
    public decimal Amount { get; set; }

    public string? Message { get; set; }

    public DateTime DonatedAt { get; set; }

    public string Status { get; set; } = null!;

    public string? TransactionRef { get; set; }

    public string? GatewayResponse { get; set; }

    public DateTime? ProcessedAt { get; set; }

    [ForeignKey("CampaignId")]
    [InverseProperty("Donations")]
    public virtual Campaign Campaign { get; set; } = null!;

    [ForeignKey("DonorUserId")]
    [InverseProperty("Donations")]
    public virtual AspNetUser? DonorUser { get; set; }
}
