using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Domain.Enum
{
    public enum ReliefRequestStatus
    {
        Pending = 0,
        Verified = 1,
        Approved = 2,
        Allocated = 3,
        Delivered = 4,
        Completed = 5,
        Rejected = 6,
    }
}
