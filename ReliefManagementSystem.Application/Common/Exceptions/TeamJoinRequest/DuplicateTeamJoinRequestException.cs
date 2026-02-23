using System;

namespace ReliefManagementSystem.Application.Common.Exceptions.TeamJoinRequest
{
    public class DuplicateTeamJoinRequestException : AppException
    {
        public DuplicateTeamJoinRequestException()
            : base("Bạn đã gửi yêu cầu gia nhập cho đội này rồi",
                "TEAM_JOIN_REQUEST_DUPLICATE",
                409)
        {
        }
    }
}