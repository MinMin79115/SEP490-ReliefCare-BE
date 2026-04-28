using ReliefManagementSystem.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Domain.Entities
{
    public class Vehicle
    {
        public Guid VehicleId { get; set; } = Guid.NewGuid();

        public Guid VehicleTypeId { get; set; }
        public VehicleType VehicleType { get; set; } = null!;

        public Guid? ReliefStationId { get; set; }
        public ReliefStation? ReliefStation { get; set; }

        public string LicensePlate { get; set; } = string.Empty;

        public Guid CreatedBy { get; set; }
        public ApplicationUser Creator { get; set; } = null!;

        public Guid? TeamId { get; set; }
        public Team? Team { get; set; }

        public VehicleStatus Status { get; set; } = VehicleStatus.Free;

        public bool IsDeleted { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public ICollection<SupplyTransfer> SupplyTransfers { get; set; } = new List<SupplyTransfer>();
        public ICollection<SupplyTransferVehicle> SupplyTransferVehicles { get; set; } = new List<SupplyTransferVehicle>();
    }
}
