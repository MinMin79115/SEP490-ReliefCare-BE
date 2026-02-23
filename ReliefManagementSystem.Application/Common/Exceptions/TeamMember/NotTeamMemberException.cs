using System;

namespace ReliefManagementSystem.Application.Common.Exceptions.TeamMember
{
    public class NotTeamMemberException : AppException
    {
        public NotTeamMemberException()
            : base("Bạn chưa tham gia team nào",
                "NOT_TEAM_MEMBER",
                404)
        {
        }

        public NotTeamMemberException(string message)
            : base(message,
                "NOT_TEAM_MEMBER",
                404)
        {
        }
    }
}