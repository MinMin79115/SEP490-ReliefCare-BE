namespace ReliefManagementSystem.Application.Common.Exceptions.ReliefStationExceptions
{
    public class StationJoinRequestNotFoundException : AppException
    {
        public StationJoinRequestNotFoundException(Guid requestId)
            : base($"Không tìm thấy yêu cầu vào trạm: {requestId}", "STATION_JOIN_REQUEST_NOT_FOUND", 404)
        {
        }
    }
}
