using System;

namespace ReliefManagementSystem.Application.Common.Exceptions.Team
{
    public class DuplicateTeamMemberException : AppException
    {
        public DuplicateTeamMemberException()
            : base("Tình nguyện viên đã là thành viên của team",
                "TEAM_DUPLICATE_MEMBER",
                409)
        {
        }

        public DuplicateTeamMemberException(string volunteerName)
            : base($"Tình nguyện viên {volunteerName} đã là thành viên của team",
                "TEAM_DUPLICATE_MEMBER",
                409)
        {
        }
    }
}