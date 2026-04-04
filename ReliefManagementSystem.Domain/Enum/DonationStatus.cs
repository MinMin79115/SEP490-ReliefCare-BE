using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Domain.Enum
{
    public enum DonationStatus
    {
        /// <summary>Chờ xác nhận thanh toán từ cổng thanh toán.</summary>
        Pending = 0,

        /// <summary>Giao dịch thành công, tiền đã về tài khoản chiến dịch.</summary>
        Completed = 1,

        /// <summary>Giao dịch thất bại (từ chối, lỗi hệ thống, hết hạn…).</summary>
        Failed = 2,

        /// <summary>Người dùng/chủ hệ thống đã huỷ giao dịch.</summary>
        Cancelled = 3,

        /// <summary>Link thanh toán đã hết hạn.</summary>
        Expired = 4,

        /// <summary>Đã hoàn tiền lại cho người donate.</summary>
        Refunded = 5
    }
}
