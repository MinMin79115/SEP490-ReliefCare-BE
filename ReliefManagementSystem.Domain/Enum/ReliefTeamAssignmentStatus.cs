using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Domain.Enum
{
    public enum ReliefTeamAssignmentStatus
    {
        Active = 1,        // Đang hoạt động tại trạm
        Transferred = 2,   // Đã điều chuyển sang trạm khác
        Suspended = 3,     // Tạm dừng (thiếu người, chờ lệnh...)
        Completed = 4,     // Hoàn thành nhiệm vụ tại trạm
        Cancelled = 5      // Hủy gán (chưa kịp triển khai)
    }
}
