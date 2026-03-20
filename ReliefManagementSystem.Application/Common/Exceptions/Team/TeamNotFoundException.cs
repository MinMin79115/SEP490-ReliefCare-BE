using System;

namespace ReliefManagementSystem.Application.Common.Exceptions.Team
{
    public class TeamNotFoundException : AppException
    {
        public TeamNotFoundException()
            : base("Không tìm thấy đội",
                "TEAM_NOT_FOUND",
                404)
        {
        }

        public TeamNotFoundException(Guid teamId)
            : base($"Không tìm thấy đội với ID: {teamId}",
                "TEAM_NOT_FOUND",
                404)
        {
        }
    }
}