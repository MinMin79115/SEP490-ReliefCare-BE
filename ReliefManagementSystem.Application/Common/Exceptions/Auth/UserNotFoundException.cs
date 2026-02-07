using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Application.Common.Exceptions.Auth
{
    public class UserNotFoundException : AppException
    {
        public UserNotFoundException(string email)
            : base($"User '{email}' not found",
                "AUTH_USER_NOT_FOUND",
                404)
        {
        }
    }
}
