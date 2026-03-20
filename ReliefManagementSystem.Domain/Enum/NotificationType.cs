using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Domain.Enum
{
    public enum NotificationType
    {
        // ── RescueRequest events ───────────────────────────────────
        RescueRequestCreated    = 1,  // Yêu cầu cứu hộ mới được gửi
        RescueRequestVerified   = 2,  // Đã xác minh
        RescueRequestAssigned   = 3,  // Đã phân công đội cứu hộ
        RescueRequestInProgress = 4,  // Đang thực hiện cứu hộ
        RescueRequestCompleted  = 5,  // Hoàn thành
        RescueRequestCancelled  = 6,  // Đã hủy

        // ── ReliefRequest events ───────────────────────────────────
        ReliefRequestCreated    = 11, // Yêu cầu cứu trợ mới được gửi
        ReliefRequestVerified   = 12, // Đã xác minh
        ReliefRequestApproved   = 13, // Đã phê duyệt
        ReliefRequestAllocated  = 14, // Đã phân bổ hàng hóa
        ReliefRequestDelivered  = 15, // Đã giao hàng
        ReliefRequestCompleted  = 16, // Hoàn thành
        ReliefRequestRejected   = 17, // Bị từ chối

        // ── SupplyTransfer events ──────────────────────────────────
        SupplyTransferCreated   = 21, // Phiếu vận chuyển kho mới
        SupplyTransferApproved  = 22, // Phiếu được duyệt
        SupplyTransferReceived  = 23, // Trạm đích nhận hàng
        SupplyTransferCancelled = 24, // Phiếu bị hủy

        // ── Khác ───────────────────────────────────────────────────
        General = 99
    }
}
