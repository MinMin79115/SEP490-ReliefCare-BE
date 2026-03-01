using System;

namespace ReliefManagementSystem.Application.Common.Exceptions.TeamJoinRequest
{
    public class TeamJoinRequestNotFoundException : AppException
    {
        public TeamJoinRequestNotFoundException()
            : base("Không tìm thấy yêu cầu",
                "TEAM_JOIN_REQUEST_NOT_FOUND",
                404)
        {
        }

        public TeamJoinRequestNotFoundException(Guid requestId)
            : base($"Không tìm thấy yêu cầu với ID: {requestId}",
                "TEAM_JOIN_REQUEST_NOT_FOUND",
                404)
        {
        }
    }
}