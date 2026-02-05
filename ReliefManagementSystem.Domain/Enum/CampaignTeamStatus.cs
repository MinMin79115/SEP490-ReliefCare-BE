using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Domain.Enum
{
    public enum CampaignTeamStatus
    {
        Invited = 0,    // được mời
        Accepted = 1,   // đã nhận
        Active = 2,     // đang hoạt động
        Completed = 3,  // đã hoàn thành
        Withdrawn = 4,   // rút lui
        Cancelled = 5   // bị hủy
    }

}
