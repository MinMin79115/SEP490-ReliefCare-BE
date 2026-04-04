using ReliefManagementSystem.Application.Common.Exceptions;

namespace ReliefManagementSystem.Application.Common.Exceptions.ReliefStationExceptions
{
    public class StationAlreadyHasModeratorException : AppException
    {
        public StationAlreadyHasModeratorException(Guid stationId)
            : base($"Trạm cứu trợ với ID {stationId} đã có Moderator phụ trách (trưởng trạm). Một trạm chỉ được có 1 Moderator.",
                "STATION_ALREADY_HAS_MODERATOR",
                400)
        {
        }
    }
}
