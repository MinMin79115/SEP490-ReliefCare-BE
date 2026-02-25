using System;

namespace ReliefManagementSystem.Application.Common.Exceptions.Volunteer
{
    public class VolunteerAlreadyInTeamException : AppException
    {
        public VolunteerAlreadyInTeamException()
            : base("Bạn đã là thành viên đội này rồi",
                "VOLUNTEER_ALREADY_IN_TEAM",
                409)
        {
        }

        public VolunteerAlreadyInTeamException(string volunteerName, string teamName)
            : base($"Tình nguyện viên '{volunteerName}' đã là thành viên của đội '{teamName}'",
                "VOLUNTEER_ALREADY_IN_TEAM",
                409)
        {
        }
    }
}