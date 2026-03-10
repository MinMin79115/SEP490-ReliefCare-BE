namespace ReliefManagementSystem.Application.Common.Exceptions.ReliefStationExceptions
{
    /// <summary>
    /// Ném ra khi LocationId truyền vào không đúng cấp yêu cầu
    /// (ví dụ: tạo trạm tỉnh nhưng truyền LocationId cấp Region).
    /// </summary>
    public class InvalidLocationLevelException : AppException
    {
        public InvalidLocationLevelException(string expected)
            : base(
                $"LocationId phải thuộc cấp {expected}. Vui lòng kiểm tra lại LocationId.",
                "INVALID_LOCATION_LEVEL",
                400)
        { }
    }
}
