using ReliefManagementSystem.Domain.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Domain.Entities
{
    public class ReliefStation
    {
        public Guid ReliefStationId { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; } = null!;

        public Guid LocationId { get; set; }
        public string? Address { get; set; }
        public string? ContactNumber { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public Guid CreatedBy { get; set; }

        public ReliefStationLevel Level { get; set; }
        public double Longitude { get; set; }
        public double Latitude { get; set; }

        public bool IsActive { get; set; } = true;

        public Guid? ParentReliefStationId { get; set; }
        public ReliefStation? ParentStation { get; set; }
        public ICollection<ReliefStation> ChildStations { get; set; } = new List<ReliefStation>();

        public ReliefStationStatus Status { get; set; }

        public Location Location { get; set; } = null!;

        /// <summary>Danh sách Moderator được gán vào trạm này (có thể có 1 IsStationHead = true).</summary>
        public ICollection<ModeratorProfile> Moderators { get; set; } = new List<ModeratorProfile>();

        public ICollection<Inventory> Inventories { get; set; } = new List<Inventory>();

        public ICollection<ReliefStationTeam> ReliefStationTeams { get; set; } = new List<ReliefStationTeam>();

        public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();

        /// <summary>Các phiếu vận chuyển hàng MÀ trạm này xuất đi</summary>
        public ICollection<SupplyTransfer> OutboundTransfers { get; set; } = new List<SupplyTransfer>();

        /// <summary>Các phiếu vận chuyển hàng MÀ trạm này nhận vào</summary>
        public ICollection<SupplyTransfer> InboundTransfers { get; set; } = new List<SupplyTransfer>();
    }
}
