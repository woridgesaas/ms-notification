using ms_notification.Models;

namespace ms_notification.Repositories;

public interface INotificationRepository
{
    Task AddNotificationAsync(Notification notification);
}
