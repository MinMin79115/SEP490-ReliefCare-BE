using System;

namespace ReliefManagementSystem.Application.Common.Exceptions.TeamMember
{
    public class TeamMemberNotFoundException : AppException
    {
        public TeamMemberNotFoundException()
            : base("Thành viên không tồn tại trong đội",
                "TEAM_MEMBER_NOT_FOUND",
                404)
        {
        }

        public TeamMemberNotFoundException(string message)
            : base(message,
                "TEAM_MEMBER_NOT_FOUND",
                404)
        {
        }
    }
}