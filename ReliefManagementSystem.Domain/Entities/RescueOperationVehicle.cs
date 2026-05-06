using System;

namespace ReliefManagementSystem.Domain.Entities
{
    public class RescueOperationVehicle
    {
        public Guid RescueOperationVehicleId { get; set; }
        public Guid RescueOperationId { get; set; }
        public Guid VehicleId { get; set; }
        public bool IsPrimary { get; set; }
        public DateTime AssignedAt { get; set; }
        public Guid? AssignedBy { get; set; }
        public DateTime? ReleasedAt { get; set; }
        public string? Note { get; set; }

        public RescueOperation RescueOperation { get; set; } = default!;
        public Vehicle Vehicle { get; set; } = default!;
    }
}
