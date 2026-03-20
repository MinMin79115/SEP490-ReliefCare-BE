using System;

namespace ReliefManagementSystem.Application.Common.Exceptions.TeamJoinRequest
{
    public class UnauthorizedRequestActionException : AppException
    {
        public UnauthorizedRequestActionException(string action)
            : base($"Chỉ có điều phối team mới có thể {action} yêu cầu",
                "TEAM_JOIN_REQUEST_UNAUTHORIZED",
                403)
        {
        }

        public UnauthorizedRequestActionException()
            : base("Bạn không có quyền thực hiện hành động này",
                "TEAM_JOIN_REQUEST_UNAUTHORIZED",
                403)
        {
        }
    }
}