using Microsoft.AspNetCore.SignalR;
using ReliefManagementSystem.Application.Common.Interface;
using ReliefManagementSystem.Domain.Entities;

namespace ReliefManagementSystem.Infrastructure.Security
{
    public class NotificationRealtimePublisher : INotificationRealtimePublisher
    {
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationRealtimePublisher(IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task PublishAsync(Notification notification)
        {
            await _hubContext.Clients
                .User(notification.RecipientId.ToString())
                .SendAsync("ReceiveNotification", notification);
        }
    }
}
