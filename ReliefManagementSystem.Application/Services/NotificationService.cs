using ReliefManagementSystem.Application.Common.Interface;
using ReliefManagementSystem.Application.Features.Notification;
using ReliefManagementSystem.Application.Interface;
using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Domain.Enum;
using System.Text.Json;

namespace ReliefManagementSystem.Application.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly INotificationRealtimePublisher _notificationRealtimePublisher;
        private readonly ICurrentUserService _currentUserService;

        public NotificationService(
            IUnitOfWork unitOfWork,
            INotificationRealtimePublisher notificationRealtimePublisher,
            ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _notificationRealtimePublisher = notificationRealtimePublisher;
            _currentUserService = currentUserService;
        }

        public async Task CreateAsync(
            Guid recipientId,
            NotificationType type,
            string title,
            string message,
            Guid? referenceId,
            string? referenceType,
            string? metadataJson,
            CancellationToken cancellationToken)
        {
            var notification = BuildNotification(recipientId, type, title, message, referenceId, referenceType, metadataJson);
            await _unitOfWork.Notifications.AddAsync(notification);
            await _unitOfWork.Notifications.InvalidateUnreadCountCacheAsync(recipientId, cancellationToken);
        }

        public async Task<Notification> CreateAndPushAsync(
            Guid recipientId,
            NotificationType type,
            string title,
            string message,
            Guid? referenceId,
            string? referenceType,
            string? metadataJson,
            CancellationToken cancellationToken)
        {
            var notification = BuildNotification(recipientId, type, title, message, referenceId, referenceType, metadataJson);
            await _unitOfWork.Notifications.AddAsync(notification);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await PushAsync(notification);
            return notification;
        }

        public async Task<List<Notification>> CreateManyAndPushAsync(
            IEnumerable<Guid> recipientIds,
            NotificationType type,
            string title,
            string message,
            Guid? referenceId,
            string? referenceType,
            string? metadataJson,
            CancellationToken cancellationToken)
        {
            var uniqueRecipients = recipientIds.Distinct().ToList();
            if (uniqueRecipients.Count == 0)
            {
                return new List<Notification>();
            }

            var notifications = uniqueRecipients
                .Select(recipientId => BuildNotification(recipientId, type, title, message, referenceId, referenceType, metadataJson))
                .ToList();

            foreach (var notification in notifications)
            {
                await _unitOfWork.Notifications.AddAsync(notification);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            foreach (var notification in notifications)
            {
                await PushAsync(notification);
                await _unitOfWork.Notifications.InvalidateUnreadCountCacheAsync(notification.RecipientId, cancellationToken);
            }

            return notifications;
        }

        public async Task PushAsync(Notification notification)
        {
            await _notificationRealtimePublisher.PublishAsync(notification);
        }

        public async Task<NotificationListResponseDto> GetMyNotificationsAsync(
            int pageNumber = 1,
            int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            var userId = _currentUserService.UserId
                ?? throw new UnauthorizedAccessException("User not authenticated.");

            var safePageNumber = pageNumber <= 0 ? 1 : pageNumber;
            var safePageSize = pageSize <= 0 ? 20 : pageSize;

            var notifications = await _unitOfWork.Notifications.GetUserNotificationsAsync(userId);
            var totalCount = notifications.Count;
            var items = notifications
                .Skip((safePageNumber - 1) * safePageSize)
                .Take(safePageSize)
                .Select(MapNotification)
                .ToList();

            return new NotificationListResponseDto
            {
                TotalCount = totalCount,
                PageNumber = safePageNumber,
                PageSize = safePageSize,
                Data = items
            };
        }

        public async Task<UnreadCountResponseDto> GetUnreadCountAsync(CancellationToken cancellationToken = default)
        {
            var userId = _currentUserService.UserId
                ?? throw new UnauthorizedAccessException("User not authenticated.");

            var unreadCount = await _unitOfWork.Notifications.GetUnreadCountAsync(userId, cancellationToken);
            return new UnreadCountResponseDto { UnreadCount = unreadCount };
        }

        public async Task<MarkReadResponseDto> MarkAsReadAsync(Guid notificationId, CancellationToken cancellationToken = default)
        {
            var userId = _currentUserService.UserId
                ?? throw new UnauthorizedAccessException("User not authenticated.");

            var notification = await _unitOfWork.Notifications.GetByIdForRecipientAsync(notificationId, userId, cancellationToken)
                ?? throw new KeyNotFoundException("Notification not found.");

            if (!notification.IsRead)
            {
                notification.IsRead = true;
                notification.ReadAt = DateTime.UtcNow;
                await _unitOfWork.Notifications.UpdateAsync(notification);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await _unitOfWork.Notifications.InvalidateUnreadCountCacheAsync(userId, cancellationToken);
            }

            return new MarkReadResponseDto
            {
                NotificationId = notification.NotificationId,
                IsRead = notification.IsRead,
                ReadAt = notification.ReadAt
            };
        }

        public async Task<MarkAllReadResponseDto> MarkAllAsReadAsync(CancellationToken cancellationToken = default)
        {
            var userId = _currentUserService.UserId
                ?? throw new UnauthorizedAccessException("User not authenticated.");

            var updatedCount = await _unitOfWork.Notifications.MarkAllAsReadAsync(userId, DateTime.UtcNow, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new MarkAllReadResponseDto { UpdatedCount = updatedCount };
        }

        private static Notification BuildNotification(
            Guid recipientId,
            NotificationType type,
            string title,
            string message,
            Guid? referenceId,
            string? referenceType,
            string? metadataJson)
        {
            return new Notification
            {
                NotificationId = Guid.NewGuid(),
                RecipientId = recipientId,
                Type = type,
                Title = title,
                Message = message,
                ReferenceId = referenceId,
                ReferenceType = referenceType,
                MetadataJson = metadataJson
            };
        }

        private static NotificationItemResponseDto MapNotification(Notification notification)
        {
            var metadata = ParseMetadata(notification.MetadataJson);

            return new NotificationItemResponseDto
            {
                NotificationId = notification.NotificationId,
                Type = notification.Type,
                Title = notification.Title,
                Message = notification.Message,
                ReferenceId = notification.ReferenceId,
                ReferenceType = notification.ReferenceType,
                MetadataJson = notification.MetadataJson,
                Metadata = metadata,
                AttachmentCount = metadata.AttachmentCount,
                ThumbnailUrls = metadata.ThumbnailUrls,
                IsRead = notification.IsRead,
                CreatedAt = notification.CreatedAt,
                ReadAt = notification.ReadAt
            };
        }

        private static NotificationMetadataDto ParseMetadata(string? metadataJson)
        {
            if (string.IsNullOrWhiteSpace(metadataJson))
            {
                return new NotificationMetadataDto();
            }

            try
            {
                return JsonSerializer.Deserialize<NotificationMetadataDto>(metadataJson)
                    ?? new NotificationMetadataDto();
            }
            catch
            {
                return new NotificationMetadataDto();
            }
        }
    }
}
