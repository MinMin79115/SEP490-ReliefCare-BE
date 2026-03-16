namespace ReliefManagementSystem.Application.Common.Exceptions.ReliefStationExceptions
{
    public class RejectionReasonRequiredException : AppException
    {
        public RejectionReasonRequiredException()
            : base("Lý do từ chối là bắt buộc", "RELIEF_STATION_REJECTION_REASON_REQUIRED", 400)
        {
        }
    }
}
