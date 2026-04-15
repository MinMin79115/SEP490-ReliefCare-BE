using ReliefManagementSystem.Domain.Enum;
using ReliefManagementSystem.Domain.Entities.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Domain.Entities
{
    public class ReliefStation : AuditableEntity
    {
        public Guid ReliefStationId { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; } = null!;

        public Guid LocationId { get; set; }
        public string? Address { get; set; }
        public string? ContactNumber { get; set; }

        public ReliefStationLevel Level { get; set; }
        public double Longitude { get; set; }
        public double Latitude { get; set; }

        /// <summary>Bán kính phục vụ mặc định của trạm (km) dùng để gán rescue request.</summary>
        public double CoverageRadiusKm { get; set; } = 30;

        public ReliefStationStatus ReliefStationStatus { get; set; }

        public Location Location { get; set; } = null!;

        public ICollection<CampaignStation> CampaignStations { get; set; } = new List<CampaignStation>();

        /// <summary>Danh sách Moderator được gán vào trạm này (có thể có 1 IsStationHead = true).</summary>
        public ICollection<ModeratorProfile> Moderators { get; set; } = new List<ModeratorProfile>();

        public ICollection<Inventory> Inventories { get; set; } = new List<Inventory>();

        public ICollection<ReliefStationTeam> ReliefStationTeams { get; set; } = new List<ReliefStationTeam>();
        public ICollection<StationJoinRequest> StationJoinRequests { get; set; } = new List<StationJoinRequest>();

        public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();

        /// <summary>Các phiếu vận chuyển hàng MÀ trạm này xuất đi</summary>
        public ICollection<SupplyTransfer> OutboundTransfers { get; set; } = new List<SupplyTransfer>();

        /// <summary>Các phiếu vận chuyển hàng MÀ trạm này nhận vào</summary>
        public ICollection<SupplyTransfer> InboundTransfers { get; set; } = new List<SupplyTransfer>();

        public ICollection<InKindDonation> ReceivedInKindDonations { get; set; } = new List<InKindDonation>();


    }
}
