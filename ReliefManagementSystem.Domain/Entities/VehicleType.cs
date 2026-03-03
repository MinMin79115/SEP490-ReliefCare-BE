using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ReliefManagementSystem.Domain.Entities;

[Index("TypeName", Name = "IX_VehicleTypes_TypeName", IsUnique = true)]
public partial class VehicleType
{
    [Key]
    public Guid VehicleTypeId { get; set; }

    [StringLength(100)]
    public string TypeName { get; set; } = null!;

    public int DefaultCapacity { get; set; }

    public string? Description { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    [InverseProperty("VehicleType")]
    public virtual ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
}
