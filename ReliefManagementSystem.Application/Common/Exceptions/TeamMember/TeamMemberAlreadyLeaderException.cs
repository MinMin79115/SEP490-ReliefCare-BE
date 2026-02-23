using System;

namespace ReliefManagementSystem.Application.Common.Exceptions.TeamMember
{
    public class TeamMemberAlreadyLeaderException : AppException
    {
        public TeamMemberAlreadyLeaderException()
            : base("Thành viên đã là Leader rồi",
                "TEAM_MEMBER_ALREADY_LEADER",
                400)
        {
        }
    }
}