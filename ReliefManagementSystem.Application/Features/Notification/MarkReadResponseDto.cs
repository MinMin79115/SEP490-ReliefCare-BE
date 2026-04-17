namespace ReliefManagementSystem.Application.Features.Notification
{
    public class MarkReadResponseDto
    {
        public Guid NotificationId { get; set; }
        public bool IsRead { get; set; }
        public DateTime? ReadAt { get; set; }
    }
}
