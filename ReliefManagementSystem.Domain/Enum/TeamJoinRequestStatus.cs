using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Domain.Enum
{
    public enum TeamJoinRequestStatus
    {
        //status of a volunteer's request to join a team
        Pending = 1,
        Approved = 2,
        Rejected = 3,
        Cancelled = 4
    }
}
