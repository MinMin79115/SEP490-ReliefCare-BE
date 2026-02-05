using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Domain.Enum
{
    public enum CampaignTaskStatus
    {
        Planned = 0,        // Đã lên kế hoạch
        InProgress = 1,     // Team đang làm
        Blocked = 2,        // Bị chặn (thiếu hàng, thiếu người)
        Completed = 3,      // Team hoàn thành
        Cancelled = 4       // Hủy
    }
}
