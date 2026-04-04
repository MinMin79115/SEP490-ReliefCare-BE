namespace ReliefManagementSystem.Application.Common.Exceptions.ReliefStationExceptions
{
    public class OnlyStationHeadCanManageAssignmentsException : AppException
    {
        public OnlyStationHeadCanManageAssignmentsException()
            : base("Chỉ trưởng trạm mới có quyền duyệt/gán team vào trạm", "RELIEF_STATION_ASSIGNMENT_FORBIDDEN", 403)
        {
        }
    }
}
