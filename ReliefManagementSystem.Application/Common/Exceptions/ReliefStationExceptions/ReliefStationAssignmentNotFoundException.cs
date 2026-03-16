namespace ReliefManagementSystem.Application.Common.Exceptions.ReliefStationExceptions
{
    public class ReliefStationAssignmentNotFoundException : AppException
    {
        public ReliefStationAssignmentNotFoundException(Guid stationId, Guid teamId)
            : base($"Không tìm thấy quan hệ team {teamId} tại trạm {stationId}",
                "RELIEF_STATION_TEAM_ASSIGNMENT_NOT_FOUND",
                404)
        {
        }
    }
}
