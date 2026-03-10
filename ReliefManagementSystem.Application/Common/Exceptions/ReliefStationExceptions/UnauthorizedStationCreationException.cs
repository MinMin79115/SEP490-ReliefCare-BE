namespace ReliefManagementSystem.Application.Common.Exceptions.ReliefStationExceptions
{
    /// <summary>
    /// Ném ra khi user hiện tại không có quyền tạo trạm ở cấp yêu cầu
    /// (ví dụ: user không có ManagerProfile hoặc không phải vai trò Manager).
    /// </summary>
    public class UnauthorizedStationCreationException : AppException
    {
        public UnauthorizedStationCreationException()
            : base(
                "Bạn không có quyền tạo trạm ở cấp này. Chỉ Manager được phép thực hiện thao tác này.",
                "UNAUTHORIZED_STATION_CREATION",
                403)
        { }
    }
}
