// Services/INotifService.cs
using ms_notification.DTO;

namespace ms_notification.Services;

public interface INotifService
{
    Task CreateNotificationAsync(CreateNotificationRequest dto, string cin);
    Task<List<NotificationResponse>> GetMyNotificationsAsync(string cin);
    Task<bool> MarkAsReadAsync(int notificationId, string cin);
    Task MarkAllAsReadAsync(string cin);
}