using ReliefManagementSystem.Application.Common.Interface;
using ReliefManagementSystem.Application.Interface;
using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace ReliefManagementSystem.Application.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationService(
            IUnitOfWork unitOfWork,
            IHubContext<NotificationHub> hubContext)
        {
            _unitOfWork = unitOfWork;
            _hubContext = hubContext;
        }

        public async Task CreateAsync(
            Guid recipientId,
            NotificationType type,
            string title,
            string message,
            Guid? referenceId,
            string? referenceType,
            CancellationToken cancellationToken)
        {
            var notification = new Notification
            {
                NotificationId = Guid.NewGuid(),
                RecipientId = recipientId,
                Type = type,
                Title = title,
                Message = message,
                ReferenceId = referenceId,
                ReferenceType = referenceType
            };

            await _unitOfWork.Notifications.AddAsync(notification);
        }

        public async Task PushAsync(Notification notification)
        {
            await _hubContext.Clients
                .User(notification.RecipientId.ToString())
                .SendAsync("ReceiveNotification", notification);
        }
    }
}
