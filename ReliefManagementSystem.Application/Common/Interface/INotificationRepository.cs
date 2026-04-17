using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using ReliefManagementSystem.Domain.Entities;

namespace ReliefManagementSystem.Application.Common.Interface
{
    public interface INotificationRepository : IGenericRepository<Notification>
    {
        Task<List<Notification>> GetUserNotificationsAsync(Guid userId);
        Task<int> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<Notification?> GetByIdForRecipientAsync(Guid notificationId, Guid recipientId, CancellationToken cancellationToken = default);
        Task<int> MarkAllAsReadAsync(Guid userId, DateTime readAtUtc, CancellationToken cancellationToken = default);
    }
}
