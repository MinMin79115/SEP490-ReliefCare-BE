using System;

namespace ReliefManagementSystem.Application.Common.Exceptions.Team
{
    public class CannotRemoveLeaderException : AppException
    {
        public CannotRemoveLeaderException()
            : base("Không thể xoá trưởng nhóm hiện tại. Phải đổi trưởng nhóm trước.",
                "TEAM_CANNOT_REMOVE_LEADER",
                400)
        {
        }
    }
}