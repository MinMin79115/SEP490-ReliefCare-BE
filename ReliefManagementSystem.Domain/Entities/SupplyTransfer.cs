using ReliefManagementSystem.Domain.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ReliefManagementSystem.Domain.Entities
{
    /// <summary>
    /// Phiếu vận chuyển hàng cứu trợ giữa các trạm theo phân cấp:
    /// Trạm Vùng (Regional) → Trạm Tỉnh (Province) → Trạm Địa phương (Local)
    /// </summary>
    public class SupplyTransfer
    {
        public Guid SupplyTransferId { get; set; }

        /// <summary>
        /// Mã phiếu vận chuyển, ví dụ: TRF-20260304-001
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string TransferCode { get; set; } = null!;

        // ── Hai đầu trạm ───────────────────────────────────────────
        /// <summary>Trạm xuất hàng (nguồn)</summary>
        public Guid SourceStationId { get; set; }

        /// <summary>Trạm nhận hàng (đích)</summary>
        public Guid DestinationStationId { get; set; }

        // ── Trạng thái & thời gian ──────────────────────────────────
        public SupplyTransferStatus Status { get; set; } = SupplyTransferStatus.Pending;

        /// <summary>Thời điểm tạo phiếu yêu cầu</summary>
        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

        /// <summary>Thời điểm phiếu được duyệt</summary>
        public DateTime? ApprovedAt { get; set; }

        /// <summary>Thời điểm hàng xuất khỏi kho nguồn</summary>
        public DateTime? ShippedAt { get; set; }

        /// <summary>Thời điểm trạm đích xác nhận đã nhận hàng</summary>
        public DateTime? ReceivedAt { get; set; }

        // ── Người tạo / người duyệt ─────────────────────────────────
        /// <summary>Người tạo phiếu</summary>
        public Guid RequestedBy { get; set; }

        /// <summary>Người duyệt phiếu (nullable – chưa duyệt)</summary>
        public Guid? ApprovedBy { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        // ── Navigation properties ────────────────────────────────────
        public ReliefStation SourceStation { get; set; } = null!;
        public ReliefStation DestinationStation { get; set; } = null!;
        public ApplicationUser RequestedByUser { get; set; } = null!;
        public ApplicationUser? ApprovedByUser { get; set; }

        // ── Thông tin vận chuyển (Logistics) ──────────────────────────
        /// <summary>Xe được điều động để chở hàng cho phiếu này</summary>
        public Guid? VehicleId { get; set; }
        public Vehicle? Vehicle { get; set; }

        /// <summary>Tài xế lái xe (Account) phụ trách chuyến này</summary>
        public Guid? DriverUserId { get; set; }
        public ApplicationUser? DriverUser { get; set; }

        public ICollection<SupplyTransferItem> Items { get; set; } = new List<SupplyTransferItem>();
        public ICollection<InventoryTransaction> InventoryTransactions { get; set; } = new List<InventoryTransaction>();
    }
}
