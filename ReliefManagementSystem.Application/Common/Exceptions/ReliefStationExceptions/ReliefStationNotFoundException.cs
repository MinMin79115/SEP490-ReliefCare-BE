namespace ReliefManagementSystem.Application.Common.Exceptions.ReliefStationExceptions
{
    public class ReliefStationNotFoundException : AppException
    {
        public ReliefStationNotFoundException() 
            : base("Không tìm thấy trạm cứu trợ.", "STATION_NOT_FOUND", 404)
        { }
    }
}
