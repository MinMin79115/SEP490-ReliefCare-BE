using System;

namespace ReliefManagementSystem.Application.Common.Exceptions.Team
{
    public class UnauthorizedTeamActionException : AppException
    {
        public UnauthorizedTeamActionException(string action)
            : base($"Chỉ có người điều phối team và admin mới có thể {action}",
                "TEAM_UNAUTHORIZED_ACTION",
                403)
        {
        }

        public UnauthorizedTeamActionException()
            : base("Bạn không có quyền thực hiện hành động này trên team",
                "TEAM_UNAUTHORIZED_ACTION",
                403)
        {
        }
    }
}