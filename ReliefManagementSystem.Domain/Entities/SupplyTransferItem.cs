using System;
using System.ComponentModel.DataAnnotations;

namespace ReliefManagementSystem.Domain.Entities
{
    /// <summary>
    /// Chi tiết từng mặt hàng trong một phiếu vận chuyển (<see cref="SupplyTransfer"/>).
    /// </summary>
    public class SupplyTransferItem
    {
        public Guid SupplyTransferItemId { get; set; }

        public Guid SupplyTransferId { get; set; }

        public Guid SupplyItemId { get; set; }

        /// <summary>Số lượng yêu cầu vận chuyển</summary>
        public int RequestedQuantity { get; set; }

        /// <summary>
        /// Số lượng thực tế trạm đích nhận được.
        /// Null cho đến khi trạng thái chuyển sang Received.
        /// </summary>
        public int? ActualQuantity { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        // ── Navigation properties ────────────────────────────────────
        public SupplyTransfer SupplyTransfer { get; set; } = null!;
        public SupplyItem SupplyItem { get; set; } = null!;
    }
}
