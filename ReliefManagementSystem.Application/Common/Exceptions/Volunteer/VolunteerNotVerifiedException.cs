using System;

namespace ReliefManagementSystem.Application.Common.Exceptions.Volunteer
{
    public class VolunteerNotVerifiedException : AppException
    {
        public VolunteerNotVerifiedException()
            : base("Tình nguyện viên phải được xác minh",
                "VOLUNTEER_NOT_VERIFIED",
                400)
        {
        }

        public VolunteerNotVerifiedException(string message)
            : base(message,
                "VOLUNTEER_NOT_VERIFIED",
                400)
        {
        }
    }
}