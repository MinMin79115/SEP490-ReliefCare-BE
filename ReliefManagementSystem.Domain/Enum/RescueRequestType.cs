using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Domain.Enum
{
    public enum RescueRequestType
    {
        Normal = 0,    // Yêu cầu cứu hộ thông thường, cần xác minh trước khi dispatch
        Emergency = 1   // Yêu cầu cứu hộ khẩn cấp, bypass xác minh và dispatch ngay
    }
}
