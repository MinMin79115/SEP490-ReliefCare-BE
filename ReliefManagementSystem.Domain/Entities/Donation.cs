using ReliefManagementSystem.Domain.Enum;
using System;
using System.ComponentModel.DataAnnotations;

namespace ReliefManagementSystem.Domain.Entities
{
    /// <summary>
    /// Lưu thông tin một lần donate vào Campaign.
    /// Người donate có thể ẩn danh (IsAnonymous = true, DonorUserId = null),
    /// nhưng hệ thống luôn lưu response từ cổng thanh toán để đối soát.
    /// </summary>
    public class Donation
    {
        [Key]
        public Guid DonationId { get; set; }

        // FK đến Campaign
        public Guid CampaignId { get; set; }
        public Campaign Campaign { get; set; } = null!;

        // FK đến người donate – nullable nếu ẩn danh
        public Guid? DonorUserId { get; set; }
        public ApplicationUser? DonorUser { get; set; }

        /// <summary>
        /// True nếu người donate chọn ẩn danh.
        /// Dù ẩn danh, DonorUserId (nếu đã login) vẫn được lưu nội bộ để đối soát,
        /// nhưng sẽ không hiển thị ra ngoài.
        /// </summary>
        public bool IsAnonymous { get; set; } = false;

        /// <summary>
        /// Tên hiển thị công khai (do người donate nhập hoặc lấy từ DisplayName).
        /// Nếu IsAnonymous = true, giá trị này sẽ không được hiển thị.
        /// </summary>
        public string? DonorName { get; set; }

        /// <summary>Số tiền donate (VNĐ hoặc đơn vị tiền tệ của hệ thống).</summary>
        public decimal Amount { get; set; }

        /// <summary>Lời nhắn / lời chúc từ người donate (tuỳ chọn).</summary>
        public string? Message { get; set; }

        /// <summary>Thời điểm tạo yêu cầu donate (UTC).</summary>
        public DateTime DonatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>Trạng thái giao dịch: Pending → Completed / Failed / Refunded.</summary>
        public DonationStatus Status { get; set; } = DonationStatus.Pending;

        /// <summary>
        /// Mã tham chiếu giao dịch từ cổng thanh toán (VNPAY, MoMo, ZaloPay…).
        /// Dùng để tra cứu / đối soát với cổng thanh toán.
        /// </summary>
        public string? TransactionRef { get; set; }

        /// <summary>
        /// Response thô (JSON) từ cổng thanh toán sau khi xử lý.
        /// Lưu toàn bộ để phục vụ audit / hoàn tiền sau này.
        /// </summary>
        public string? GatewayResponse { get; set; }

        /// <summary>Thời điểm cổng thanh toán confirm xong (UTC). Null nếu chưa xử lý.</summary>
        public DateTime? ProcessedAt { get; set; }
    }
}
