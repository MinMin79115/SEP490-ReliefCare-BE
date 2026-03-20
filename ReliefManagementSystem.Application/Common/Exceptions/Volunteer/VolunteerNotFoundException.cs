using System;

namespace ReliefManagementSystem.Application.Common.Exceptions.Volunteer
{
    public class VolunteerNotFoundException : AppException
    {
        public VolunteerNotFoundException()
            : base("Không tìm thấy tình nguyện viên",
                "VOLUNTEER_NOT_FOUND",
                404)
        {
        }

        public VolunteerNotFoundException(Guid volunteerId)
            : base($"Không tìm thấy tình nguyện viên với ID: {volunteerId}",
                "VOLUNTEER_NOT_FOUND",
                404)
        {
        }
    }
}