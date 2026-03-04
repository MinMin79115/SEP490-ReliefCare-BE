using Microsoft.EntityFrameworkCore;
using ReliefManagementSystem.Domain.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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

    public VehicleStatus Status { get; set; } = VehicleStatus.Free;
    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }


    public virtual ICollection<CampaignVehicle> CampaignVehicles { get; set; } = new List<CampaignVehicle>();

    public virtual ApplicationUser Creator { get; set; } = null!;


    public virtual ReliefStation ReliefStation { get; set; } = null!;

    public virtual VehicleType VehicleType { get; set; } = null!;
}
