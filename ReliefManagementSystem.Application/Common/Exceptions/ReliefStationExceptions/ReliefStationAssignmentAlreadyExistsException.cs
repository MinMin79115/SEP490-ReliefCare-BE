namespace ReliefManagementSystem.Application.Common.Exceptions.ReliefStationExceptions
{
    public class ReliefStationAssignmentAlreadyExistsException : AppException
    {
        public ReliefStationAssignmentAlreadyExistsException(Guid stationId, Guid teamId)
            : base($"Team {teamId} đã tồn tại quan hệ với trạm {stationId}",
                "RELIEF_STATION_TEAM_ASSIGNMENT_EXISTS",
                400)
        {
        }
    }
}
