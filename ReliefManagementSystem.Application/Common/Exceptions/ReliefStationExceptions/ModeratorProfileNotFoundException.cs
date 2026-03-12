using ReliefManagementSystem.Application.Common.Exceptions;

namespace ReliefManagementSystem.Application.Common.Exceptions.ReliefStationExceptions
{
    public class ModeratorProfileNotFoundException : AppException
    {
        public ModeratorProfileNotFoundException(Guid userId)
            : base($"Không tìm thấy hồ sơ Moderator cho User ID: {userId}. Người dùng này có thể chưa được cấp quyền Moderator.",
                "MODERATOR_PROFILE_NOT_FOUND",
                404)
        {
        }
    }
}
