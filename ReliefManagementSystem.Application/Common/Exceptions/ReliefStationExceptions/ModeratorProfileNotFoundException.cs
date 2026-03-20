namespace ReliefManagementSystem.Application.Common.Exceptions.ReliefStationExceptions
{
    public class ModeratorProfileNotFoundException : AppException
    {
        public ModeratorProfileNotFoundException()
            : base("Không tìm thấy hồ sơ moderator", "MODERATOR_PROFILE_NOT_FOUND", 404)
        {
        }

        public ModeratorProfileNotFoundException(Guid userId)
            : base($"Không tìm thấy hồ sơ moderator cho user {userId}", "MODERATOR_PROFILE_NOT_FOUND", 404)
        {
        }
    }
}
