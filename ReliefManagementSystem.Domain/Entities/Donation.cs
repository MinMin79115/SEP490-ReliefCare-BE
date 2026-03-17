using ReliefManagementSystem.Domain.Enum;
using System;
using System.ComponentModel.DataAnnotations;

namespace ReliefManagementSystem.Domain.Entities
{
    public class Donation
    {
        [Key]
        public Guid DonationId { get; set; }

        // FK đến Campaign
        public Guid CampaignId { get; set; }
        public Campaign Campaign { get; set; } = null!;

        // FK đến người donate – nullable nếu chưa đăng nhập
        public Guid? DonorUserId { get; set; }
        public ApplicationUser? DonorUser { get; set; }

        /// <summary>Tên người donate hiển thị công khai.</summary>
        public string DonorName { get; set; } = string.Empty;

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

        /// <summary>orderCode gửi sang PayOS, dùng để định danh đơn thanh toán.</summary>
        public long? PayOsOrderCode { get; set; }

        /// <summary>paymentLinkId do PayOS trả về.</summary>
        public string? PayOsPaymentLinkId { get; set; }

        /// <summary>Checkout URL do PayOS trả về khi tạo link.</summary>
        public string? CheckoutUrl { get; set; }

        /// <summary>Thời gian hết hạn link thanh toán (UTC).</summary>
        public DateTime ExpiresAt { get; set; }

        /// <summary>
        /// Response thô (JSON) từ cổng thanh toán sau khi xử lý.
        /// Lưu toàn bộ để phục vụ audit / hoàn tiền sau này.
        /// </summary>
        public string? GatewayResponse { get; set; }

        /// <summary>Thời điểm cổng thanh toán confirm xong (UTC). Null nếu chưa xử lý.</summary>
        public DateTime? ProcessedAt { get; set; }
    }
}
