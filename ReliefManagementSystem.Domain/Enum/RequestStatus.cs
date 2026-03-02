using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Domain.Enum
{
    public enum RequestStatus
    {
        Draft,      // đang điền form
        Submitted,  // đã gửi, chờ xử lý
        Verified,
        Rejected,
        InProgress, // đang được xử lý
        Resolved,   // hoàn thành
        Cancelled
    }

}
