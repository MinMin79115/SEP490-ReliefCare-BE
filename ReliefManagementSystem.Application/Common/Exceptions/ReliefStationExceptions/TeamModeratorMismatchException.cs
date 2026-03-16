namespace ReliefManagementSystem.Application.Common.Exceptions.ReliefStationExceptions
{
    public class TeamModeratorMismatchException : AppException
    {
        public TeamModeratorMismatchException()
            : base("Bạn không phải moderator quản lý team này", "TEAM_MODERATOR_MISMATCH", 403)
        {
        }
    }
}
