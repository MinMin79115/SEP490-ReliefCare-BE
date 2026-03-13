using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Domain.Enum
{
    public enum RescueOperationStatus
    {
        Pending = 0,
        Assigned = 1,
        EnRoute = 2,
        Rescuing = 3,
        Completed = 4,
        Cancelled = 5
    }
}
