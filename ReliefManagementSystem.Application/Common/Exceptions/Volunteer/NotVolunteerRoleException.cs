using System;

namespace ReliefManagementSystem.Application.Common.Exceptions.Volunteer
{
    public class NotVolunteerRoleException : AppException
    {
        public NotVolunteerRoleException()
            : base("Chỉ có tình nguyện viên mới được yêu cầu tham gia",
                "NOT_VOLUNTEER_ROLE",
                403)
        {
        }
    }
}