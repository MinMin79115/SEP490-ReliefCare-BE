namespace ReliefManagementSystem.Application.Common.Exceptions.ReliefStationExceptions
{
    public class ReliefStationInactiveException : AppException
    {
        public ReliefStationInactiveException()
            : base("Trạm hiện không hoạt động", "RELIEF_STATION_INACTIVE", 400)
        {
        }
    }
}
