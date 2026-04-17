namespace ReliefManagementSystem.Application.Features.Notification
{
    public class NotificationListResponseDto
    {
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public List<NotificationItemResponseDto> Data { get; set; } = new();
    }
}
