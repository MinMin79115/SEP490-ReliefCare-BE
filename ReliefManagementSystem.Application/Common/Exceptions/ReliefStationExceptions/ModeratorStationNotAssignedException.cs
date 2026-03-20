namespace ReliefManagementSystem.Application.Common.Exceptions.ReliefStationExceptions
{
    public class ModeratorStationNotAssignedException : AppException
    {
        public ModeratorStationNotAssignedException()
            : base("Moderator chưa được gán quản lý trạm nào", "MODERATOR_STATION_NOT_ASSIGNED", 404)
        {
        }
    }
}
