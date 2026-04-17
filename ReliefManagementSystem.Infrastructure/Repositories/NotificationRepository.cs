using Microsoft.EntityFrameworkCore;
using ReliefManagementSystem.Application.Common.Interface;
using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Infrastructure.Repositories
{
    public class NotificationRepository : GenericRepository<Notification>, INotificationRepository
    {
        public NotificationRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<List<Notification>> GetUserNotificationsAsync(Guid userId)
        {
            return await _context.Notifications
                .Where(n => n.RecipientId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<int> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _context.Notifications
                .CountAsync(n => n.RecipientId == userId && !n.IsRead, cancellationToken);
        }

        public async Task<Notification?> GetByIdForRecipientAsync(Guid notificationId, Guid recipientId, CancellationToken cancellationToken = default)
        {
            return await _context.Notifications
                .FirstOrDefaultAsync(n => n.NotificationId == notificationId && n.RecipientId == recipientId, cancellationToken);
        }

        public async Task<int> MarkAllAsReadAsync(Guid userId, DateTime readAtUtc, CancellationToken cancellationToken = default)
        {
            var unread = await _context.Notifications
                .Where(n => n.RecipientId == userId && !n.IsRead)
                .ToListAsync(cancellationToken);

            foreach (var item in unread)
            {
                item.IsRead = true;
                item.ReadAt = readAtUtc;
            }

            return unread.Count;
        }
    }
}
