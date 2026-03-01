using System;

namespace ReliefManagementSystem.Application.Common.Exceptions.TeamJoinRequest
{
    public class InvalidRequestStatusException : AppException
    {
        public InvalidRequestStatusException(string action, string currentStatus)
            : base($"Chỉ có thể {action} các yêu cầu đang chờ xử lý. Trạng thái hiện tại: {currentStatus}",
                "TEAM_JOIN_REQUEST_INVALID_STATUS",
                400)
        {
        }

        public InvalidRequestStatusException(string message)
            : base(message,
                "TEAM_JOIN_REQUEST_INVALID_STATUS",
                400)
        {
        }
    }
}