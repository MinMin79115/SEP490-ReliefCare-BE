using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Domain.Enum
{
    public enum CampaignStatus
    {
        Draft = 0,
        Active = 1,
        Suspended = 2,
        Completed = 3,
        Cancelled = 4,
        GoalsMet = 5,
        ReadyToExecute = 6,
        InProgress = 7,
        Closing = 8
    }

}
