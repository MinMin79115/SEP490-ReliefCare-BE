using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Application.Common.Exceptions.Auth
{
    public class UserLockedException : AppException
    {
        public UserLockedException()
            : base("User account is locked",
                "AUTH_USER_LOCKED",
                403)
        {
        }
    }
}
