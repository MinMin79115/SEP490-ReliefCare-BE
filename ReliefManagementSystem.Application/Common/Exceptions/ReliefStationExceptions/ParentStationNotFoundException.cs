namespace ReliefManagementSystem.Application.Common.Exceptions.ReliefStationExceptions
{
    /// <summary>
    /// Ném ra khi không tìm thấy trạm Regional cha trong vùng mà Manager phụ trách.
    /// Nguyên nhân thường gặp: LocationId (tỉnh) không thuộc vùng của Manager,
    /// hoặc trạm Regional của vùng chưa được tạo.
    /// </summary>
    public class ParentStationNotFoundException : AppException
    {
        public ParentStationNotFoundException()
            : base(
                "Không tìm thấy trạm vùng (Regional) cha tương ứng. " +
                "Hãy đảm bảo LocationId thuộc vùng bạn phụ trách và trạm vùng đã tồn tại.",
                "PARENT_STATION_NOT_FOUND",
                404)
        { }
    }
}
