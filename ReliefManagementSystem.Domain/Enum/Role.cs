using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Domain.Enum
{
    public enum Role
    {
        Admin,
        User,
        Volunteer,
        /// <summary>Has ModeratorProfile (1:1).</summary>
        Moderator,
        /// <summary>Has ManagerProfile (1:1).</summary>
        Manager
    }
}
