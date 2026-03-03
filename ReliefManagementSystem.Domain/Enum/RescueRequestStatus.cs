using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Domain.Enum
{
    public enum RescueRequestStatus
    {
        Pending,
        Verified,
        Assigned,
        InProgress,
        Completed,
        Cancelled
    }
}
