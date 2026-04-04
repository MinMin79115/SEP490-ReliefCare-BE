namespace ReliefManagementSystem.Application.Common.Exceptions.ReliefStationExceptions
{
    public class ReliefStationAssignmentNotPendingException : AppException
    {
        public ReliefStationAssignmentNotPendingException()
            : base("Yêu cầu không ở trạng thái chờ duyệt", "RELIEF_STATION_REQUEST_NOT_PENDING", 400)
        {
        }
    }
}
