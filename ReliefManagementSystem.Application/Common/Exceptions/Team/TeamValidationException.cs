namespace ReliefManagementSystem.Application.Common.Exceptions.Team
{
    public class TeamValidationException : AppException
    {
        public TeamValidationException(string message)
            : base(message, "TEAM_VALIDATION_ERROR", 400)
        {
        }
    }
}
