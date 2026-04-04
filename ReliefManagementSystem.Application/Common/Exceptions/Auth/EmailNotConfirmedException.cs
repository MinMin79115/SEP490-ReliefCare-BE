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
            : base("Email has not been confirmed. Please check your inbox and confirm your email before logging in.",
                "AUTH_EMAIL_NOT_CONFIRMED",
                403)
        {
        }
    }
}
