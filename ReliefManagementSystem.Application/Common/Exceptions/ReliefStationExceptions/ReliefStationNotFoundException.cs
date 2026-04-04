namespace ReliefManagementSystem.Application.Common.Exceptions.ReliefStationExceptions
{
    public class ReliefStationNotFoundException : AppException
    {
        public ReliefStationNotFoundException(Guid stationId)
            : base($"Không tìm thấy trạm cứu trợ với Id: {stationId}",
                "RELIEF_STATION_NOT_FOUND",
                404)
        {
        }
    }
}
