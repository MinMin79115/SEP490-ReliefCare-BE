using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Domain.Enum
{
    public enum MemberTaskStatus
    {
        Assigned = 0,       // Đã giao
        InProgress = 1,     // Đang làm
        Completed = 2,      // Hoàn thành
        Failed = 3,          // Không hoàn thành
        Cancelled = 4       // Hủy
    }
}
