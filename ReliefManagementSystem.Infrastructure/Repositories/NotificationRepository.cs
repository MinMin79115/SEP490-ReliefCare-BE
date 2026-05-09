using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using ReliefManagementSystem.Application.Common.Interface;
using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;

namespace ReliefManagementSystem.Infrastructure.Repositories
{
    public class NotificationRepository : GenericRepository<Notification>, INotificationRepository
    {
        private static readonly TimeSpan UnreadCountCacheTtl = TimeSpan.FromSeconds(60);
        private readonly IDistributedCache _cache;

        public NotificationRepository(ApplicationDbContext context, IDistributedCache cache) : base(context)
        {
            _cache = cache;
        }

        private static string GetUnreadCountCacheKey(Guid userId)
        {
            return $"reliefcare:notifications:unread-count:{userId:N}";
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
            var cacheKey = GetUnreadCountCacheKey(userId);
            var cachedCount = await _cache.GetStringAsync(cacheKey, cancellationToken);
            if (int.TryParse(cachedCount, NumberStyles.Integer, CultureInfo.InvariantCulture, out var unreadCount))
            {
                return unreadCount;
            }

            unreadCount = await _context.Notifications
                .CountAsync(n => n.RecipientId == userId && !n.IsRead, cancellationToken);

            await _cache.SetStringAsync(
                cacheKey,
                unreadCount.ToString(CultureInfo.InvariantCulture),
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = UnreadCountCacheTtl
                },
                cancellationToken);

            return unreadCount;
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

            await InvalidateUnreadCountCacheAsync(userId, cancellationToken);

            return unread.Count;
        }

        public Task InvalidateUnreadCountCacheAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return _cache.RemoveAsync(GetUnreadCountCacheKey(userId), cancellationToken);
        }
    }
}
