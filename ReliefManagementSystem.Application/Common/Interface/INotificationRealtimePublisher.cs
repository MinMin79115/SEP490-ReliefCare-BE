using ReliefManagementSystem.Domain.Entities;

namespace ReliefManagementSystem.Application.Common.Interface
{
    public interface INotificationRealtimePublisher
    {
        Task PublishAsync(Notification notification);
    }
}
