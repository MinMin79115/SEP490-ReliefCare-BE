using ReliefManagementSystem.Domain.Enum;

namespace ReliefManagementSystem.Application.Features.Notification
{
    public class RealtimeNotificationDto
    {
        public Guid NotificationId { get; set; }
        public Guid RecipientId { get; set; }
        public NotificationType Type { get; set; }
        public string Title { get; set; } = null!;
        public string? Message { get; set; }
        public Guid? ReferenceId { get; set; }
        public string? ReferenceType { get; set; }
        public string? MetadataJson { get; set; }
        public NotificationMetadataDto Metadata { get; set; } = new();
        public int AttachmentCount { get; set; }
        public List<string> ThumbnailUrls { get; set; } = new();
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ReadAt { get; set; }
    }
}
