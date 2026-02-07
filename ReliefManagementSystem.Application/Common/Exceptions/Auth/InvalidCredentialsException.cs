using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Application.Common.Exceptions.Auth
{
    public class InvalidCredentialsException : AppException
    {
        public InvalidCredentialsException()
            : base("Email or password is incorrect",
                "AUTH_INVALID_CREDENTIALS",
                401)
        {
        }
    }
}
