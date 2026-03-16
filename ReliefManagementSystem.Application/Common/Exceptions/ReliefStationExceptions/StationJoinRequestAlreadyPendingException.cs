namespace ReliefManagementSystem.Application.Common.Exceptions.ReliefStationExceptions
{
    public class StationJoinRequestAlreadyPendingException : AppException
    {
        public StationJoinRequestAlreadyPendingException()
            : base("Đã tồn tại yêu cầu chờ duyệt cho team và trạm này", "STATION_JOIN_REQUEST_ALREADY_PENDING", 400)
        {
        }
    }
}
