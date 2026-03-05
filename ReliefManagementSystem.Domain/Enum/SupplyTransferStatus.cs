using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Domain.Enum
{
    public enum SupplyTransferStatus
    {
        Pending  = 1,   // Chờ duyệt
        Approved = 2,   // Đã duyệt, chưa xuất kho
        Shipping = 3,   // Đang vận chuyển (đã xuất khỏi kho nguồn)
        Received = 4,   // Trạm đích đã nhận và xác nhận
        Cancelled = 5   // Đã hủy
    }
}
