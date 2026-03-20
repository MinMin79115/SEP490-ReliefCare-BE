using ReliefManagementSystem.Domain.Entities;
using ReliefManagementSystem.Domain.Enum;
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
            CancellationToken cancellationToken);

        Task PushAsync(Notification notification);
    }
}
