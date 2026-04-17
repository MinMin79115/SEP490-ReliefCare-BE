namespace ReliefManagementSystem.Application.Features.Notification
{
    public class NotificationMetadataDto
    {
        public int SchemaVersion { get; set; } = 1;
        public string SchemaName { get; set; } = "rescue_request_v1";
        public int AttachmentCount { get; set; }
        public List<string> ThumbnailUrls { get; set; } = new();
    }
}
