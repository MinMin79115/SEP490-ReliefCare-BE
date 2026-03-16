using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Domain.Enum
{
    public enum ReliefTeamAssignmentStatus
    {
        Pending = 0,       // Team đã gửi yêu cầu, chờ trạm duyệt
        Active = 1,        // Đang hoạt động tại trạm
        Transferred = 2,   // Đã điều chuyển sang trạm khác
        Suspended = 3,     // Tạm dừng (thiếu người, chờ lệnh...)
        InMission = 4,     // Đang thực hiện nhiệm vụ
        Completed = 5,     // Hoàn thành nhiệm vụ tại trạm
        Cancelled = 6      // Hủy gán (chưa kịp triển khai)
    }
}
