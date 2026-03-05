namespace ReliefManagementSystem.Application.Common.Exceptions.ReliefStationExceptions
{
    public class UnauthorizedModeratorAssignmentException : AppException
    {
        public UnauthorizedModeratorAssignmentException() 
            : base("Bạn không có quyền gán Moderator cho trạm này.", "UNAUTHORIZED_MODERATOR_ASSIGNMENT", 403)
        { }
    }
}
