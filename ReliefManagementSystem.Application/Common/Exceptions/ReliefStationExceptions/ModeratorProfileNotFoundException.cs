namespace ReliefManagementSystem.Application.Common.Exceptions.ReliefStationExceptions
{
    public class ModeratorProfileNotFoundException : AppException
    {
        public ModeratorProfileNotFoundException() 
            : base("Không tìm thấy hồ sơ Moderator cho người dùng này.", "MODERATOR_NOT_FOUND", 404)
        { }
    }
}
