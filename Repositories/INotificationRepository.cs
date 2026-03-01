using ms_notification.Models;

namespace ms_notification.Repositories;

public interface INotificationRepository
{
    Task AddNotificationAsync(Notification notification);
    Task<List<NotificationReceive>> GetByEmployeeIdAsync(int employeeId);
    Task<List<NotificationReceive>> GetUnreadByEmployeeIdAsync(int employeeId);
    Task<NotificationReceive?> GetReceiveAsync(int notificationId, int employeeId);
    Task UpdateReceiveAsync(NotificationReceive receive);
    Task SaveChangesAsync();
}

