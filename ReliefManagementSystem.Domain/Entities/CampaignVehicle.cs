using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ReliefManagementSystem.Domain.Entities;

[Index("AssignedDriverId", Name = "IX_CampaignVehicles_AssignedDriverId")]
[Index("CampaignId", Name = "IX_CampaignVehicles_CampaignId")]
[Index("VehicleId", Name = "IX_CampaignVehicles_VehicleId")]
public partial class CampaignVehicle
{
    [Key]
    public Guid CampaignVehicleId { get; set; }

    public Guid VehicleId { get; set; }

    public Guid CampaignId { get; set; }

    public Guid? AssignedDriverId { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public int Status { get; set; }

    public string? Note { get; set; }
    public virtual VolunteerProfile? AssignedDriver { get; set; }
    public virtual Campaign Campaign { get; set; } = null!;

    public virtual Vehicle Vehicle { get; set; } = null!;
}
