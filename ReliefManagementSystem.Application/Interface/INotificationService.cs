using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Domain.Enum;
using ReliefManagementSystem.Application.Features.Notification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Application.Interface
{
    public interface INotificationService
    {
        Task CreateAsync(
            Guid recipientId,
            NotificationType type,
            string title,
            string message,
            Guid? referenceId,
            string? referenceType,
            string? metadataJson,
            CancellationToken cancellationToken);

        Task<Notification> CreateAndPushAsync(
            Guid recipientId,
            NotificationType type,
            string title,
            string message,
            Guid? referenceId,
            string? referenceType,
            string? metadataJson,
            CancellationToken cancellationToken);

        Task<List<Notification>> CreateManyAndPushAsync(
            IEnumerable<Guid> recipientIds,
            NotificationType type,
            string title,
            string message,
            Guid? referenceId,
            string? referenceType,
            string? metadataJson,
            CancellationToken cancellationToken);

        Task<NotificationListResponseDto> GetMyNotificationsAsync(
            int pageNumber = 1,
            int pageSize = 20,
            CancellationToken cancellationToken = default);

        Task<UnreadCountResponseDto> GetUnreadCountAsync(CancellationToken cancellationToken = default);

        Task<MarkReadResponseDto> MarkAsReadAsync(Guid notificationId, CancellationToken cancellationToken = default);

        Task<MarkAllReadResponseDto> MarkAllAsReadAsync(CancellationToken cancellationToken = default);

        Task PushAsync(Notification notification);
    }
}
