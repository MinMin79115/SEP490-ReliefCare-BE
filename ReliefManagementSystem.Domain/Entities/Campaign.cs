using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ReliefManagementSystem.Domain.Entities;

[Index("CreatedBy", Name = "IX_Campaigns_CreatedBy")]
[Index("CreatedByStationId", Name = "IX_Campaigns_CreatedByStationId")]
[Index("LocationId", Name = "IX_Campaigns_LocationId")]
public partial class Campaign
{
    [Key]
    public Guid CampaignId { get; set; }

    public Guid LocationId { get; set; }

    public Guid CreatedByStationId { get; set; }

    public Guid CreatedBy { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public DateTime CreatedAt { get; set; }

    public int Status { get; set; }

    public double AreaRadiusKm { get; set; }

    public decimal BudgetSpent { get; set; }

    public decimal BudgetTotal { get; set; }

    public string? Description { get; set; }

    public decimal Latitude { get; set; }

    public decimal Longitude { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<CampaignTask> CampaignTasks { get; set; } = new List<CampaignTask>();

    public virtual ICollection<CampaignTeam> CampaignTeams { get; set; } = new List<CampaignTeam>();

    public virtual ICollection<CampaignVehicle> CampaignVehicles { get; set; } = new List<CampaignVehicle>();

    public virtual ApplicationUser Creator { get; set; } = null!;

    public virtual ReliefStation CreatedByStation { get; set; } = null!;

    public virtual ICollection<Donation> Donations { get; set; } = new List<Donation>();

    public virtual Location Location { get; set; } = null!;

    public virtual ICollection<ReliefRequest> ReliefRequests { get; set; } = new List<ReliefRequest>();

    public virtual ICollection<SupplyAllocation> SupplyAllocations { get; set; } = new List<SupplyAllocation>();
}
