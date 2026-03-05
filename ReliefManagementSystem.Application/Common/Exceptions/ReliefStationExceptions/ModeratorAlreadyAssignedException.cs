namespace ReliefManagementSystem.Application.Common.Exceptions.ReliefStationExceptions
{
    public class ModeratorAlreadyAssignedException : AppException
    {
        public ModeratorAlreadyAssignedException() 
            : base("Moderator này đã được gán vào một trạm khác. Vui lòng gỡ trước khi gán mới.", "MODERATOR_ALREADY_ASSIGNED", 400)
        { }
    }
}
