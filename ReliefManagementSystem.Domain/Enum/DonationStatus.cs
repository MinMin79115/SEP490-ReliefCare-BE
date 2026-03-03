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
        Pending,

        /// <summary>Giao dịch thành công, tiền đã về tài khoản chiến dịch.</summary>
        Completed,

        /// <summary>Giao dịch thất bại (từ chối, lỗi hệ thống, hết hạn…).</summary>
        Failed,

        /// <summary>Đã hoàn tiền lại cho người donate.</summary>
        Refunded
    }
}
