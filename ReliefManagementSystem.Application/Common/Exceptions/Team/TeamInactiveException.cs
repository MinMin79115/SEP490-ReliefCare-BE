using System;

namespace ReliefManagementSystem.Application.Common.Exceptions.Team
{
    public class TeamInactiveException : AppException
    {
        public TeamInactiveException()
            : base("Đội chưa hoạt động",
                "TEAM_INACTIVE",
                400)
        {
        }

        public TeamInactiveException(string teamName)
            : base($"Đội '{teamName}' chưa hoạt động",
                "TEAM_INACTIVE",
                400)
        {
        }
    }
}