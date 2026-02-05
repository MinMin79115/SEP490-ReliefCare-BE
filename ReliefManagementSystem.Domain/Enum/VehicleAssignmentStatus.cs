using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Domain.Enum
{
    public enum VehicleAssignmentStatus
    {
        Pending = 0,    // Chờ duyệt
        Approved = 1,   // Đã duyệt, chuẩn bị đi
        InTransit = 2,  // Đang di chuyển đi
        OnSite = 3,     // Đang tại hiện trường
        Returning = 4,  // Đang quay về
        Completed = 5,  // Đã hoàn thành nhiệm vụ
        Canceled = 6,   // Đã hủy lệnh
        Incident = 7    // Gặp sự cố dọc đường
    }
}
