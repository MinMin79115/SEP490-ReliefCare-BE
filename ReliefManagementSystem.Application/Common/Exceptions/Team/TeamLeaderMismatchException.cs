namespace ReliefManagementSystem.Application.Common.Exceptions.Team
{
    public class TeamLeaderMismatchException : AppException
    {
        public TeamLeaderMismatchException()
            : base("Chỉ team leader mới có thể gửi yêu cầu xin vào trạm", "TEAM_LEADER_MISMATCH", 403)
        {
        }
    }
}
