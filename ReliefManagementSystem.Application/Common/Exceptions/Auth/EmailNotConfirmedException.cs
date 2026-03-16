using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Application.Common.Exceptions.Auth
{
    public class EmailNotConfirmedException : AppException
    {
        public EmailNotConfirmedException()
            : base("Your email is not confirmed",
                "AUTH_INVALID_CREDENTIALS",
                401)
        {
        }
    }
}
