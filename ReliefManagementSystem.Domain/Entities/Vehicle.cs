using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ReliefManagementSystem.Domain.Entities;

[Index("CreatedBy", Name = "IX_Vehicles_CreatedBy")]
[Index("LicensePlate", Name = "IX_Vehicles_LicensePlate", IsUnique = true)]
[Index("ReliefStationId", Name = "IX_Vehicles_ReliefStationId")]
[Index("VehicleTypeId", Name = "IX_Vehicles_VehicleTypeId")]
public partial class Vehicle
{
    [Key]
    public Guid VehicleId { get; set; }

    public Guid VehicleTypeId { get; set; }

    public Guid ReliefStationId { get; set; }

    [StringLength(20)]
    public string LicensePlate { get; set; } = null!;

    public Guid CreatedBy { get; set; }

    public string? TeamUsed { get; set; }

    public int Status { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    [InverseProperty("Vehicle")]
    public virtual ICollection<CampaignVehicle> CampaignVehicles { get; set; } = new List<CampaignVehicle>();

    [ForeignKey("CreatedBy")]
    [InverseProperty("Vehicles")]
    public virtual AspNetUser CreatedByNavigation { get; set; } = null!;

    [ForeignKey("ReliefStationId")]
    [InverseProperty("Vehicles")]
    public virtual ReliefStation ReliefStation { get; set; } = null!;

    [ForeignKey("VehicleTypeId")]
    [InverseProperty("Vehicles")]
    public virtual VehicleType VehicleType { get; set; } = null!;
}
