using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Domain.Entities
{
    public class SupplyTransferVehicle
    {
        public Guid SupplyTransferVehicleId { get; set; }
        public Guid SupplyTransferId { get; set; }
        public Guid VehicleId { get; set; }
        public Guid? DriverUserId { get; set; }
        public SupplyTransferVehicleStatus Status { get; set; } = SupplyTransferVehicleStatus.Assigned;
        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
        public DateTime? DepartedAt { get; set; }
        public DateTime? ArrivedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime? CancelledAt { get; set; }
        public string? Note { get; set; }

        public SupplyTransfer SupplyTransfer { get; set; } = null!;
        public Vehicle Vehicle { get; set; } = null!;
        public ApplicationUser? DriverUser { get; set; }
    }
}
